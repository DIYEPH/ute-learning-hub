using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Message.Commands.CreateMessage;
using UteLearningHub.Application.Features.Message.Commands.DeleteMessage;
using UteLearningHub.Application.Features.Message.Commands.MarkMessageAsRead;
using UteLearningHub.Application.Features.Message.Commands.PinMessage;
using UteLearningHub.Application.Features.Message.Commands.UpdateMessage;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using DomainMessage = UteLearningHub.Domain.Entities.Message;

namespace UteLearningHub.Infrastructure.Services.Message;

public class MessageService(
    IMessageRepository messageRepository,
    IConversationRepository conversationRepository,
    IFileUsageService fileUsageService,
    IIdentityService identityService,
    ICurrentUserService currentUserService,
    IUserService userService,
    IDateTimeProvider dateTimeProvider,
    IMessageQueueProducer messageQueueProducer,
    IMessageHubService messageHubService,
    IConversationSystemMessageService systemMessageService) : IMessageService
{
    public async Task<MessageDto> CreateAsync(CreateMessageCommand request, CancellationToken ct = default)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to send messages");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        var conversation = await conversationRepository.GetByIdWithDetailsAsync(
            request.ConversationId, disableTracking: false, ct);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException($"Conversation with id {request.ConversationId} not found");

        if (conversation.ConversationStatus != ConversationStatus.Active)
            throw new BadRequestException("Conversation is not active");

        if (!conversation.Members.Any(m => m.UserId == userId && !m.IsDeleted))
            throw new ForbiddenException("You are not a member of this conversation");

        // Kiểm tra tin nhắn cha (reply) - global filter đã loại bỏ tin đã xóa
        if (request.ParentId.HasValue)
        {
            var parentMessage = await messageRepository.GetByIdAsync(
                request.ParentId.Value, disableTracking: true, ct);

            if (parentMessage == null || parentMessage.IsDeleted ||
                parentMessage.ConversationId != request.ConversationId)
                throw new NotFoundException($"Parent message with id {request.ParentId.Value} not found");
        }

        var message = new DomainMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            ParentId = request.ParentId,
            Content = request.Content,
            IsEdit = false,
            IsPined = false,
            CreatedById = userId,
            CreatedAt = dateTimeProvider.OffsetNow
        };

        if (request.FileIds != null && request.FileIds.Any())
        {
            var files = await fileUsageService.EnsureFilesAsync(request.FileIds, ct);
            foreach (var file in files)
                message.MessageFiles.Add(new Domain.Entities.MessageFile
                {
                    MessageId = message.Id,
                    FileId = file.Id
                });
        }

        messageRepository.Add(message);
        conversation.LastMessage = message.Id;
        conversation.UpdatedAt = dateTimeProvider.OffsetNow;
        conversationRepository.Update(conversation);

        await messageRepository.UnitOfWork.SaveChangesAsync(ct);

        var createdMessage = await messageRepository.GetByIdWithDetailsAsync(
            message.Id, disableTracking: true, ct);

        if (createdMessage == null)
            throw new NotFoundException("Failed to create message");

        var sender = await identityService.FindByIdAsync(userId);
        if (sender == null)
            throw new UnauthorizedException();

        var messageDto = CreateMessageDto(createdMessage, sender);

        // Fire-and-forget Kafka
        _ = Task.Run(async () =>
        {
            try { await messageQueueProducer.PublishMessageCreatedAsync(messageDto, ct); }
            catch { }
        }, ct);

        await messageHubService.BroadcastMessageCreatedAsync(messageDto, ct);

        return messageDto;
    }

    public async Task<MessageDto> UpdateAsync(UpdateMessageCommand request, CancellationToken ct = default)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update messages");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        var message = await messageRepository.GetByIdWithDetailsAsync(
            request.Id, disableTracking: false, ct);

        if (message == null || message.IsDeleted)
            throw new NotFoundException($"Message with id {request.Id} not found");

        if (message.ConversationId != request.ConversationId)
            throw new BadRequestException("Message does not belong to the specified conversation");

        if (message.CreatedById != userId)
            throw new UnauthorizedException("You don't have permission to update this message");

        var conversation = await conversationRepository.GetByIdAsync(
            request.ConversationId, disableTracking: true, ct);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException("Conversation not found");

        if (conversation.ConversationStatus != ConversationStatus.Active)
            throw new BadRequestException("Conversation is not active");

        message.Content = request.Content;
        message.IsEdit = true;
        message.UpdatedById = userId;
        message.UpdatedAt = dateTimeProvider.OffsetNow;

        messageRepository.Update(message);
        await messageRepository.UnitOfWork.SaveChangesAsync(ct);

        var updatedMessage = await messageRepository.GetByIdWithDetailsAsync(
            message.Id, disableTracking: true, ct);

        if (updatedMessage == null)
            throw new NotFoundException("Failed to update message");

        var sender = await identityService.FindByIdAsync(userId);
        if (sender == null)
            throw new UnauthorizedException();

        var messageDto = CreateMessageDto(updatedMessage, sender);

        // Fire-and-forget Kafka
        _ = Task.Run(async () =>
        {
            try { await messageQueueProducer.PublishMessageUpdatedAsync(messageDto, ct); }
            catch { }
        }, ct);

        await messageHubService.BroadcastMessageUpdatedAsync(messageDto, ct);

        return messageDto;
    }

    public async Task DeleteAsync(DeleteMessageCommand request, CancellationToken ct = default)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to delete messages");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        var message = await messageRepository.GetByIdAsync(
            request.Id, disableTracking: false, ct);

        if (message == null || message.IsDeleted)
            throw new NotFoundException($"Message with id {request.Id} not found");

        if (message.ConversationId != request.ConversationId)
            throw new BadRequestException("Message does not belong to the specified conversation");

        var conversation = await conversationRepository.GetByIdWithDetailsAsync(
            request.ConversationId, disableTracking: false, ct);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException("Conversation not found");

        var isOwner = message.CreatedById == userId;
        var isAdmin = currentUserService.IsInRole("Admin");
        var trustLevel = await userService.GetTrustLevelAsync(userId, ct);
        var isConversationOwnerOrDeputy = conversation.Members.Any(m =>
            m.UserId == userId &&
            (m.ConversationMemberRoleType == ConversationMemberRoleType.Owner ||
             m.ConversationMemberRoleType == ConversationMemberRoleType.Deputy) &&
            !m.IsDeleted);

        if (!isOwner && !isAdmin && !isConversationOwnerOrDeputy &&
            (!trustLevel.HasValue || trustLevel.Value < TrustLever.Moderator))
            throw new UnauthorizedException("You don't have permission to delete this message");

        // Soft delete - global filter sẽ tự động ẩn khỏi query
        var messageId = message.Id;
        var conversationId = message.ConversationId;

        message.DeletedById = userId;
        message.DeletedAt = dateTimeProvider.OffsetUtcNow;
        message.IsDeleted = true;

        messageRepository.Update(message);
        await messageRepository.UnitOfWork.SaveChangesAsync(ct);

        // Fire-and-forget Kafka
        _ = Task.Run(async () =>
        {
            try { await messageQueueProducer.PublishMessageDeletedAsync(messageId, conversationId, ct); }
            catch { }
        }, ct);

        await messageHubService.BroadcastMessageDeletedAsync(messageId, conversationId, ct);
    }

    public async Task PinAsync(PinMessageCommand request, CancellationToken ct = default)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to pin messages");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        var message = await messageRepository.GetByIdAsync(
            request.Id, disableTracking: false, ct);

        if (message == null || message.IsDeleted)
            throw new NotFoundException($"Message with id {request.Id} not found");

        if (message.ConversationId != request.ConversationId)
            throw new BadRequestException("Message does not belong to the specified conversation");

        var conversation = await conversationRepository.GetByIdWithDetailsAsync(
            request.ConversationId, disableTracking: false, ct);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException("Conversation not found");

        if (conversation.ConversationStatus != ConversationStatus.Active)
            throw new BadRequestException("Conversation is not active");

        var member = conversation.Members.FirstOrDefault(m =>
            m.UserId == userId && !m.IsDeleted);

        if (member == null)
            throw new ForbiddenException("You are not a member of this conversation");

        var isOwnerOrDeputy = member.ConversationMemberRoleType == ConversationMemberRoleType.Owner ||
                              member.ConversationMemberRoleType == ConversationMemberRoleType.Deputy;

        if (!isOwnerOrDeputy && !conversation.IsAllowMemberPin)
            throw new UnauthorizedException("You don't have permission to pin messages in this conversation");

        message.IsPined = request.IsPined;
        message.UpdatedById = userId;
        message.UpdatedAt = dateTimeProvider.OffsetNow;

        messageRepository.Update(message);
        await messageRepository.UnitOfWork.SaveChangesAsync(ct);

        await systemMessageService.CreateAsync(
            conversation.Id, userId,
            request.IsPined ? MessageType.MessagePinned : MessageType.MessageUnpinned,
            message.Id, ct);

        var updatedMessage = await messageRepository.GetByIdWithDetailsAsync(
            message.Id, disableTracking: true, ct);

        if (updatedMessage != null)
        {
            var sender = await identityService.FindByIdAsync(updatedMessage.CreatedById);
            var messageDto = CreateMessageDto(updatedMessage, sender);

            // Fire-and-forget Kafka
            _ = Task.Run(async () =>
            {
                try
                {
                    if (request.IsPined)
                        await messageQueueProducer.PublishMessagePinnedAsync(messageDto, ct);
                    else
                        await messageQueueProducer.PublishMessageUnpinnedAsync(messageDto, ct);
                }
                catch { }
            }, ct);

            if (request.IsPined)
                await messageHubService.BroadcastMessagePinnedAsync(messageDto, ct);
            else
                await messageHubService.BroadcastMessageUnpinnedAsync(messageDto, ct);
        }
    }

    public async Task MarkAsReadAsync(MarkMessageAsReadCommand request, CancellationToken ct = default)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to mark messages as read");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        var message = await messageRepository.GetByIdAsync(
            request.MessageId, disableTracking: true, ct);

        if (message == null || message.IsDeleted)
            throw new NotFoundException($"Message with id {request.MessageId} not found");

        if (message.ConversationId != request.ConversationId)
            throw new BadRequestException("Message does not belong to the specified conversation");

        var conversation = await conversationRepository.GetByIdWithDetailsAsync(
            request.ConversationId, disableTracking: false, ct);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException("Conversation not found");

        if (conversation.ConversationStatus != ConversationStatus.Active)
            throw new BadRequestException("Conversation is not active");

        var member = conversation.Members.FirstOrDefault(m =>
            m.UserId == userId && !m.IsDeleted);

        if (member == null)
            throw new ForbiddenException("You are not a member of this conversation");

        // Xử lý race condition: nhiều request cùng lúc chỉ cần 1 thành công
        if (member.LastReadMessageId != request.MessageId)
        {
            try
            {
                member.LastReadMessageId = request.MessageId;
                await conversationRepository.UnitOfWork.SaveChangesAsync(ct);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException) { }
        }
    }

    private static MessageDto CreateMessageDto(Domain.Entities.Message message, AppUserDto? sender)
    {
        return new MessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            ParentId = message.ParentId,
            Content = message.Content,
            IsEdit = message.IsEdit,
            IsPined = message.IsPined,
            Type = message.Type,
            CreatedById = message.CreatedById,
            SenderName = sender?.FullName ?? string.Empty,
            SenderAvatarUrl = sender?.AvatarUrl,
            Files = message.MessageFiles.Select(mf => new MessageFileDto
            {
                FileId = mf.File.Id,
                FileName = mf.File.FileName,
                FileSize = mf.File.FileSize,
                MimeType = mf.File.MimeType
            }).ToList(),
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt
        };
    }
}
