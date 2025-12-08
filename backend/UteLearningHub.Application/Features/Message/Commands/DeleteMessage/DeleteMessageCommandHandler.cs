using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Message.Commands.DeleteMessage;

public class DeleteMessageCommandHandler : IRequestHandler<DeleteMessageCommand, Unit>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMessageQueueProducer _messageQueueProducer;

    public DeleteMessageCommandHandler(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService,
        IUserService userService,
        IDateTimeProvider dateTimeProvider,
        IMessageQueueProducer messageQueueProducer)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
        _userService = userService;
        _dateTimeProvider = dateTimeProvider;
        _messageQueueProducer = messageQueueProducer;
    }

    public async Task<Unit> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to delete messages");

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

        var isOwner = message.CreatedById == userId;
        var isAdmin = _currentUserService.IsInRole("Admin");
        var trustLevel = await _userService.GetTrustLevelAsync(userId, cancellationToken);

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

        // Soft delete using repository method
        await _messageRepository.DeleteAsync(message, userId, cancellationToken);
        await _messageRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Publish message deleted event (async, không block response)
        _ = Task.Run(async () =>
        {
            try
            {
                await _messageQueueProducer.PublishMessageDeletedAsync(messageId, conversationId, cancellationToken);
            }
            catch
            {
                // Log error nhưng không throw để không ảnh hưởng đến response
                // Logger có thể được inject nếu cần
            }
        }, cancellationToken);

        return Unit.Value;
    }
}