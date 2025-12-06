using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Message.Commands.PinMessage;

public class PinMessageCommandHandler : IRequestHandler<PinMessageCommand, Unit>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IIdentityService _identityService;
    private readonly IConversationSystemMessageService _systemMessageService;
    private readonly IMessageQueueProducer _messageQueueProducer;

    public PinMessageCommandHandler(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IIdentityService identityService,
        IConversationSystemMessageService systemMessageService,
        IMessageQueueProducer messageQueueProducer)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _identityService = identityService;
        _systemMessageService = systemMessageService;
        _messageQueueProducer = messageQueueProducer;
    }

    public async Task<Unit> Handle(PinMessageCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to pin messages");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var message = await _messageRepository.GetByIdAsync(
            request.Id, 
            disableTracking: false, 
            cancellationToken);

        if (message == null || message.IsDeleted)
            throw new NotFoundException($"Message with id {request.Id} not found");

        if (message.ConversationId != request.ConversationId)
            throw new BadRequestException("Message does not belong to the specified conversation");
            
        var conversation = await _conversationRepository.GetByIdWithDetailsAsync(
            request.ConversationId, 
            disableTracking: false, 
            cancellationToken);

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
        message.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _messageRepository.UpdateAsync(message, cancellationToken);
        await _messageRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await _systemMessageService.CreateAsync(
            conversation.Id,
            userId,
            request.IsPined ? MessageType.MessagePinned : MessageType.MessageUnpinned,
            message.Id,
            cancellationToken);

        // Reload message with details để tạo MessageDto
        var updatedMessage = await _messageRepository.GetByIdWithDetailsAsync(
            message.Id,
            disableTracking: true,
            cancellationToken);

        if (updatedMessage != null)
        {
            // Get sender information
            var sender = await _identityService.FindByIdAsync(updatedMessage.CreatedById);
            var messageDto = new MessageDto
            {
                Id = updatedMessage.Id,
                ConversationId = updatedMessage.ConversationId,
                ParentId = updatedMessage.ParentId,
                Content = updatedMessage.Content,
                IsEdit = updatedMessage.IsEdit,
                IsPined = updatedMessage.IsPined,
                Type = updatedMessage.Type,
                CreatedById = updatedMessage.CreatedById,
                SenderName = sender?.FullName ?? string.Empty,
                SenderAvatarUrl = sender?.AvatarUrl,
                Files = updatedMessage.MessageFiles.Select(mf => new MessageFileDto
                {
                    FileId = mf.File.Id,
                    FileSize = mf.File.FileSize,
                    MimeType = mf.File.MimeType
                }).ToList(),
                CreatedAt = updatedMessage.CreatedAt,
                UpdatedAt = updatedMessage.UpdatedAt
            };

            // Publish message pinned/unpinned event (async, không block response)
            _ = Task.Run(async () =>
            {
                try
                {
                    if (request.IsPined)
                    {
                        await _messageQueueProducer.PublishMessagePinnedAsync(messageDto, cancellationToken);
                    }
                    else
                    {
                        await _messageQueueProducer.PublishMessageUnpinnedAsync(messageDto, cancellationToken);
                    }
                }
                catch
                {
                    // Log error nhưng không throw để không ảnh hưởng đến response
                    // Logger có thể được inject nếu cần
                }
            }, cancellationToken);
        }

        return Unit.Value;
    }

}