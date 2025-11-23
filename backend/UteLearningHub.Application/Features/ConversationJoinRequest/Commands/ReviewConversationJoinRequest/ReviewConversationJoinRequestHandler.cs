using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using DomainConversationMember = UteLearningHub.Domain.Entities.ConversationMember;

namespace UteLearningHub.Application.Features.ConversationJoinRequest.Commands.ReviewConversationJoinRequest;

public class ReviewConversationJoinRequestHandler : IRequestHandler<ReviewConversationJoinRequestCommand, Unit>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReviewConversationJoinRequestHandler(
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(ReviewConversationJoinRequestCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to review join requests");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Get join request with conversation
        var joinRequest = await _conversationRepository.GetJoinRequestsQueryable()
            .Include(r => r.Conversation)
            .FirstOrDefaultAsync(r => r.Id == request.JoinRequestId && !r.IsDeleted, cancellationToken);

        if (joinRequest == null)
            throw new NotFoundException($"Join request with id {request.JoinRequestId} not found");

        // Only Private conversations have join requests
        if (joinRequest.Conversation.ConversationType != ConversitionType.Private)
            throw new BadRequestException("Join requests are only available for private conversations");

        // Check permission: Admin, Moderator, or Owner of the conversation
        var isAdmin = _currentUserService.IsInRole("Admin");    

        var isOwner = await _conversationRepository.GetQueryableSet()
            .Where(c => c.Id == joinRequest.ConversationId)
            .SelectMany(c => c.Members)
            .AnyAsync(m => m.UserId == userId 
                        && m.ConversationMemberRoleType == ConversationMemberRoleType.Owner 
                        && !m.IsDeleted, cancellationToken);

        var canReview = isAdmin || isOwner;

        if (!canReview)
            throw new UnauthorizedException("Only administrators or conversation owners can review join requests");

        // Update review information
        joinRequest.ReviewStatus = request.ReviewStatus;
        joinRequest.ReviewedById = userId;
        joinRequest.ReviewedAt = _dateTimeProvider.OffsetNow;
        joinRequest.ReviewNote = request.ReviewNote;

        // If approved, add user to conversation members
        if (request.ReviewStatus == ReviewStatus.Approved)
        {
            // Check if user is already a member
            var isMember = await _conversationRepository.GetQueryableSet()
                .Where(c => c.Id == joinRequest.ConversationId)
                .SelectMany(c => c.Members)
                .AnyAsync(m => m.UserId == joinRequest.CreatedById && !m.IsDeleted, cancellationToken);

            if (!isMember)
            {
                // Load conversation with members
                var conversation = await _conversationRepository.GetByIdWithDetailsAsync(
                    joinRequest.ConversationId, 
                    disableTracking: false, 
                    cancellationToken);

                if (conversation == null)
                    throw new NotFoundException($"Conversation with id {joinRequest.ConversationId} not found");

                // Add user as member
                var member = new DomainConversationMember
                {
                    Id = Guid.NewGuid(),
                    UserId = joinRequest.CreatedById,
                    ConversationId = joinRequest.ConversationId,
                    ConversationMemberRoleType = ConversationMemberRoleType.Member,
                    IsMuted = false,
                    CreatedAt = _dateTimeProvider.OffsetNow
                };

                conversation.Members.Add(member);
                await _conversationRepository.UpdateAsync(conversation, cancellationToken);
            }
        }

        await _conversationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
