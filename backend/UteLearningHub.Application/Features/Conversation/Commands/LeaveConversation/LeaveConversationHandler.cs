using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Commands.LeaveConversation;

public class LeaveConversationHandler : IRequestHandler<LeaveConversationCommand>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;

    public LeaveConversationHandler(
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService)
    {
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
    }

    public async Task Handle(LeaveConversationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to leave a conversation");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var conversation = await _conversationRepository.GetByIdWithDetailsAsync(
            request.ConversationId,
            disableTracking: false,
            cancellationToken);

        if (conversation == null || conversation.IsDeleted)
            throw new NotFoundException($"Conversation with id {request.ConversationId} not found");

        var member = conversation.Members.FirstOrDefault(m => m.UserId == userId && !m.IsDeleted);
        if (member == null)
            throw new BadRequestException("You are not a member of this conversation");

        if (member.ConversationMemberRoleType == ConversationMemberRoleType.Owner)
        {
            var otherMembers = conversation.Members.Where(m => m.UserId != userId && !m.IsDeleted).ToList();
            if (otherMembers.Any())
                throw new BadRequestException("You cannot leave as the owner. Please transfer ownership to another member or delete the conversation.");
        }

        await _conversationRepository.RemoveMemberAsync(request.ConversationId, userId, cancellationToken);
        await _conversationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}

