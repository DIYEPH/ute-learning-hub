using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Commands.DeleteConversation;

public class DeleteConversationHandler : IRequestHandler<DeleteConversationCommand>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteConversationHandler(
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to delete a conversation");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var conversation = await _conversationRepository.GetByIdWithDetailsAsync(
            request.Id,
            disableTracking: false,
            cancellationToken);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException($"Conversation with id {request.Id} not found");

        // Check permission: Admin or Owner (Deputy cannot delete)
        var isAdmin = _currentUserService.IsInRole("Admin");
        var isOwner = conversation.Members.Any(m =>
            m.UserId == userId &&
            m.ConversationMemberRoleType == ConversationMemberRoleType.Owner &&
            !m.IsDeleted);

        if (!isAdmin && !isOwner)
            throw new UnauthorizedException("Only administrators or conversation owners can delete conversations");

        conversation.DeletedAt = _dateTimeProvider.OffsetUtcNow;
        conversation.DeletedById = userId;
        conversation.IsDeleted = true;

        _conversationRepository.Update(conversation);
        await _conversationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}