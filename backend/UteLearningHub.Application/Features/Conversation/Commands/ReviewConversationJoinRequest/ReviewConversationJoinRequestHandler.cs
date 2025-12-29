using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using DomainConversationMember = UteLearningHub.Domain.Entities.ConversationMember;

namespace UteLearningHub.Application.Features.Conversation.Commands.ReviewConversationJoinRequest;

public class ReviewConversationJoinRequestHandler : IRequestHandler<ReviewConversationJoinRequestCommand>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IIdentityService _identityService;
    private readonly IConversationSystemMessageService _systemMessageService;

    public ReviewConversationJoinRequestHandler(
        IConversationRepository conversationRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IIdentityService identityService,
        IConversationSystemMessageService systemMessageService)
    {
        _conversationRepository = conversationRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _identityService = identityService;
        _systemMessageService = systemMessageService;
    }

    public async Task Handle(ReviewConversationJoinRequestCommand request, CancellationToken cancellationToken)
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

        // Only Private visibility conversations have join requests
        if (joinRequest.Conversation.Visibility != ConversationVisibility.Private)
            throw new BadRequestException("Join requests are only available for private conversations");

        // Check permission: Only Owner or Deputy can review
        var isOwnerOrDeputy = await _conversationRepository.GetQueryableSet()
            .Where(c => c.Id == joinRequest.ConversationId)
            .SelectMany(c => c.Members)
            .AnyAsync(m => m.UserId == userId
                        && (m.ConversationMemberRoleType == ConversationMemberRoleType.Owner ||
                            m.ConversationMemberRoleType == ConversationMemberRoleType.Deputy)
                        && !m.IsDeleted, cancellationToken);

        if (!isOwnerOrDeputy)
            throw new UnauthorizedException("Only conversation owners or deputies can review join requests");

        // Update review information
        joinRequest.Status = request.Status;
        joinRequest.ReviewedById = userId;
        joinRequest.ReviewNote = request.ReviewNote;

        // If approved, add user to conversation members
        var approvedAndAdded = false;
        if (request.Status == ContentStatus.Approved)
        {
            // Check if user is already a member
            var isMember = await _conversationRepository.GetQueryableSet()
                .Where(c => c.Id == joinRequest.ConversationId)
                .SelectMany(c => c.Members)
                .AnyAsync(m => m.UserId == joinRequest.CreatedById && !m.IsDeleted, cancellationToken);

            if (!isMember)
            {
                // Add user as member directly (avoid navigation property to prevent concurrency issues)
                var member = new DomainConversationMember
                {
                    Id = Guid.NewGuid(),
                    UserId = joinRequest.CreatedById,
                    ConversationId = joinRequest.ConversationId,
                    ConversationMemberRoleType = ConversationMemberRoleType.Member,
                    IsMuted = false,
                    CreatedAt = _dateTimeProvider.OffsetNow
                };

                await _conversationRepository.AddMemberAsync(member, cancellationToken);
                approvedAndAdded = true;
            }
        }

        await _conversationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        if (approvedAndAdded)
        {
            await _systemMessageService.CreateAsync(
                joinRequest.ConversationId,
                userId,
                MessageType.JoinRequestApproved,
                joinRequest.CreatedById,
                cancellationToken);
        }
    }
}
