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

        // Validate conversation exists and user is a member
        var conversation = await conversationRepository.GetByIdWithDetailsAsync(
            request.ConversationId,
            disableTracking: false,
            ct);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException($"Conversation with id {request.ConversationId} not found");

        if (conversation.ConversationStatus != ConversationStatus.Active)
            throw new BadRequestException("Conversation is not active");

        // Check if user is a member
        var isMember = conversation.Members.Any(m =>
            m.UserId == userId && !m.IsDeleted);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this conversation");

        // Validate parent message if provided
        if (request.ParentId.HasValue)
        {
            var parentMessage = await messageRepository.GetByIdAsync(
                request.ParentId.Value,
                disableTracking: true,
                ct);

            if (parentMessage == null || parentMessage.IsDeleted ||
                parentMessage.ConversationId != request.ConversationId)
                throw new NotFoundException($"Parent message with id {request.ParentId.Value} not found");
        }

        // Create message
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
            {
                message.MessageFiles.Add(new Domain.Entities.MessageFile
                {
                    MessageId = message.Id,
                    FileId = file.Id
                });
            }
        }

        messageRepository.Add(message);

        // Update conversation's LastMessage
        conversation.LastMessage = message.Id;
        conversation.UpdatedAt = dateTimeProvider.OffsetNow;
        conversationRepository.Update(conversation);

        await messageRepository.UnitOfWork.SaveChangesAsync(ct);

        // Reload message with details
        var createdMessage = await messageRepository.GetByIdWithDetailsAsync(
            message.Id,
            disableTracking: true,
            ct);

        if (createdMessage == null)
            throw new NotFoundException("Failed to create message");

        // Get sender information
        var sender = await identityService.FindByIdAsync(userId);
        if (sender == null)
            throw new UnauthorizedException();

        var messageDto = CreateMessageDto(createdMessage, sender);

        // Broadcast via Kafka (if enabled) AND via SignalR directly
        await messageQueueProducer.PublishMessageCreatedAsync(messageDto, ct);
        await messageHubService.BroadcastMessageCreatedAsync(messageDto, ct);

        return messageDto;
    }

    public async Task<MessageDto> UpdateAsync(UpdateMessageCommand request, CancellationToken ct = default)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update messages");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        // Get message with details
        var message = await messageRepository.GetByIdWithDetailsAsync(
            request.Id,
            disableTracking: false,
            ct);

        if (message == null || message.IsDeleted)
            throw new NotFoundException($"Message with id {request.Id} not found");

        if (message.ConversationId != request.ConversationId)
            throw new BadRequestException("Message does not belong to the specified conversation");

        // Only owner can update
        if (message.CreatedById != userId)
            throw new UnauthorizedException("You don't have permission to update this message");

        // Validate conversation is still active
        var conversation = await conversationRepository.GetByIdAsync(
            request.ConversationId,
            disableTracking: true,
            ct);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException("Conversation not found");

        if (conversation.ConversationStatus != ConversationStatus.Active)
            throw new BadRequestException("Conversation is not active");

        // Update message
        message.Content = request.Content;
        message.IsEdit = true;
        message.UpdatedById = userId;
        message.UpdatedAt = dateTimeProvider.OffsetNow;

        messageRepository.Update(message);
        await messageRepository.UnitOfWork.SaveChangesAsync(ct);

        // Reload message with details
        var updatedMessage = await messageRepository.GetByIdWithDetailsAsync(
            message.Id,
            disableTracking: true,
            ct);

        if (updatedMessage == null)
            throw new NotFoundException("Failed to update message");

        // Get sender information
        var sender = await identityService.FindByIdAsync(userId);
        if (sender == null)
            throw new UnauthorizedException();

        var messageDto = CreateMessageDto(updatedMessage, sender);

        _ = Task.Run(async () =>
        {
            try
            {
                await messageQueueProducer.PublishMessageUpdatedAsync(messageDto, ct);
            }
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
            request.Id,
            disableTracking: false,
            ct);

        if (message == null || message.IsDeleted)
            throw new NotFoundException($"Message with id {request.Id} not found");

        if (message.ConversationId != request.ConversationId)
            throw new BadRequestException("Message does not belong to the specified conversation");

        var conversation = await conversationRepository.GetByIdWithDetailsAsync(
            request.ConversationId,
            disableTracking: false,
            ct);

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

        var canDelete = isOwner ||
                       isAdmin ||
                       isConversationOwnerOrDeputy ||
                       (trustLevel.HasValue && trustLevel.Value >= TrustLever.Moderator);

        if (!canDelete)
            throw new UnauthorizedException("You don't have permission to delete this message");

        var messageId = message.Id;
        var conversationId = message.ConversationId;

        message.DeletedById = userId;
        message.DeletedAt = dateTimeProvider.OffsetUtcNow;
        message.IsDeleted = true;

        messageRepository.Update(message);
        await messageRepository.UnitOfWork.SaveChangesAsync(ct);

        _ = Task.Run(async () =>
        {
            try
            {
                await messageQueueProducer.PublishMessageDeletedAsync(messageId, conversationId, ct);
            }
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
            request.Id,
            disableTracking: false,
            ct);

        if (message == null || message.IsDeleted)
            throw new NotFoundException($"Message with id {request.Id} not found");

        if (message.ConversationId != request.ConversationId)
            throw new BadRequestException("Message does not belong to the specified conversation");

        var conversation = await conversationRepository.GetByIdWithDetailsAsync(
            request.ConversationId,
            disableTracking: false,
            ct);

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
        var canPin = isOwnerOrDeputy || conversation.IsAllowMemberPin;

        if (!canPin)
            throw new UnauthorizedException("You don't have permission to pin messages in this conversation");

        message.IsPined = request.IsPined;
        message.UpdatedById = userId;
        message.UpdatedAt = dateTimeProvider.OffsetNow;

        messageRepository.Update(message);
        await messageRepository.UnitOfWork.SaveChangesAsync(ct);

        await systemMessageService.CreateAsync(
            conversation.Id,
            userId,
            request.IsPined ? MessageType.MessagePinned : MessageType.MessageUnpinned,
            message.Id,
            ct);

        // Reload message with details để tạo MessageDto
        var updatedMessage = await messageRepository.GetByIdWithDetailsAsync(
            message.Id,
            disableTracking: true,
            ct);

        if (updatedMessage != null)
        {
            var sender = await identityService.FindByIdAsync(updatedMessage.CreatedById);
            var messageDto = CreateMessageDto(updatedMessage, sender);

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

        // Validate message exists
        var message = await messageRepository.GetByIdAsync(
            request.MessageId,
            disableTracking: true,
            ct);

        if (message == null || message.IsDeleted)
            throw new NotFoundException($"Message with id {request.MessageId} not found");

        if (message.ConversationId != request.ConversationId)
            throw new BadRequestException("Message does not belong to the specified conversation");

        // Validate conversation exists and get member info
        var conversation = await conversationRepository.GetByIdWithDetailsAsync(
            request.ConversationId,
            disableTracking: false,
            ct);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException("Conversation not found");

        if (conversation.ConversationStatus != ConversationStatus.Active)
            throw new BadRequestException("Conversation is not active");

        // Check if user is a member
        var member = conversation.Members.FirstOrDefault(m =>
            m.UserId == userId && !m.IsDeleted);

        if (member == null)
            throw new ForbiddenException("You are not a member of this conversation");

        // Update LastReadMessageId (only if the new message is newer than current)
        if (member.LastReadMessageId == null ||
            (message.CreatedAt > (await messageRepository.GetByIdAsync(
                member.LastReadMessageId.Value,
                disableTracking: true,
                ct))?.CreatedAt))
        {
            member.LastReadMessageId = request.MessageId;
        }

        conversationRepository.Update(conversation);
        await conversationRepository.UnitOfWork.SaveChangesAsync(ct);
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
                FileSize = mf.File.FileSize,
                MimeType = mf.File.MimeType
            }).ToList(),
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt
        };
    }
}
