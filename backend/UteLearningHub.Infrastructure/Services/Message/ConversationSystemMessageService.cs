using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Infrastructure.Services.Message;

public class ConversationSystemMessageService : IConversationSystemMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IIdentityService _identityService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMessageQueueProducer _messageQueueProducer;
    private readonly IMessageHubService _messageHubService;

    public ConversationSystemMessageService(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        IIdentityService identityService,
        IDateTimeProvider dateTimeProvider,
        IMessageQueueProducer messageQueueProducer,
        IMessageHubService messageHubService)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _identityService = identityService;
        _dateTimeProvider = dateTimeProvider;
        _messageQueueProducer = messageQueueProducer;
        _messageHubService = messageHubService;
    }

    public async Task<MessageDto> CreateAsync(
        Guid conversationId,
        Guid actorId,
        MessageType type,
        Guid? parentId = null,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(
            conversationId,
            disableTracking: false,
            cancellationToken);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException($"Conversation with id {conversationId} not found");

        var message = new Domain.Entities.Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            ParentId = parentId,
            Content = string.Empty,
            Type = type,
            IsEdit = false,
            IsPined = false,
            CreatedById = actorId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        _messageRepository.Add(message);

        conversation.LastMessage = message.Id;
        conversation.UpdatedAt = _dateTimeProvider.OffsetNow;
        _conversationRepository.Update(conversation);

        await _messageRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var actor = await _identityService.FindByIdAsync(actorId)
                    ?? throw new NotFoundException($"User with id {actorId} not found");

        var messageDto = new MessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            ParentId = message.ParentId,
            Content = message.Content,
            IsEdit = message.IsEdit,
            IsPined = message.IsPined,
            Type = message.Type,
            CreatedById = message.CreatedById,
            SenderName = actor.FullName,
            SenderAvatarUrl = actor.AvatarUrl,
            Files = Array.Empty<MessageFileDto>(),
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt
        };

        await _messageQueueProducer.PublishMessageCreatedAsync(messageDto, cancellationToken);
        await _messageHubService.BroadcastMessageCreatedAsync(messageDto, cancellationToken);

        return messageDto;
    }
}


