using MediatR;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Application.Services.Identity;

namespace UteLearningHub.Application.Features.Message.Commands.MarkMessageAsRead;

public class MarkMessageAsReadCommandHandler : IRequestHandler<MarkMessageAsReadCommand, Unit>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public MarkMessageAsReadCommandHandler(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to mark messages as read");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Validate message exists
        var message = await _messageRepository.GetByIdAsync(
            request.MessageId, 
            disableTracking: true, 
            cancellationToken);

        if (message == null || message.IsDeleted)
            throw new NotFoundException($"Message with id {request.MessageId} not found");

        // Validate conversation exists and get member info
        var conversation = await _conversationRepository.GetByIdWithDetailsAsync(
            message.ConversationId, 
            disableTracking: false, 
            cancellationToken);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException("Conversation not found");

        if (conversation.ConversationStatus != Domain.Constaints.Enums.ConversationStatus.Active)
            throw new BadRequestException("Conversation is not active");

        // Check if user is a member
        var member = conversation.Members.FirstOrDefault(m =>
            m.UserId == userId && !m.IsDeleted);

        if (member == null)
            throw new ForbidenException("You are not a member of this conversation");

        // Validate message belongs to this conversation
        if (message.ConversationId != conversation.Id)
            throw new BadRequestException("Message does not belong to this conversation");

        // Update LastReadMessageId (only if the new message is newer than current)
        // This ensures we don't go backwards in read status
        if (member.LastReadMessageId == null || 
            (message.CreatedAt > (await _messageRepository.GetByIdAsync(
                member.LastReadMessageId.Value, 
                disableTracking: true, 
                cancellationToken))?.CreatedAt))
        {
            member.LastReadMessageId = request.MessageId;
        }

        await _conversationRepository.UpdateAsync(conversation, cancellationToken);
        await _conversationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}