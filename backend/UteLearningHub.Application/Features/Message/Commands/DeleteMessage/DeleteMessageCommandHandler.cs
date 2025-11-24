using MediatR;
using UteLearningHub.Application.Services.Identity;
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

    public DeleteMessageCommandHandler(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService,
        IUserService userService,
        IDateTimeProvider dateTimeProvider)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
        _userService = userService;
        _dateTimeProvider = dateTimeProvider;
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

        // Validate conversation exists
        var conversation = await _conversationRepository.GetByIdWithDetailsAsync(
            request.ConversationId, 
            disableTracking: false, 
            cancellationToken);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException("Conversation not found");

        // Check permission: owner, admin, or user with high trust level
        var isOwner = message.CreatedById == userId;
        var isAdmin = _currentUserService.IsInRole("Admin");
        var trustLevel = await _userService.GetTrustLevelAsync(userId, cancellationToken);

        // Check if user is conversation owner
        var isConversationOwner = conversation.Members.Any(m =>
            m.UserId == userId &&
            m.ConversationMemberRoleType == ConversationMemberRoleType.Owner &&
            !m.IsDeleted);

        var canDelete = isOwner || 
                       isAdmin || 
                       isConversationOwner ||
                       (trustLevel.HasValue && trustLevel.Value >= TrustLever.Moderator);

        if (!canDelete)
            throw new UnauthorizedException("You don't have permission to delete this message");

        // Soft delete
        message.IsDeleted = true;
        message.DeletedAt = _dateTimeProvider.OffsetNow;
        message.DeletedById = userId;

        await _messageRepository.UpdateAsync(message, cancellationToken);
        await _messageRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}