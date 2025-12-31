using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetMyProposals;

public class GetMyProposalsHandler : IRequestHandler<GetMyProposalsQuery, GetMyProposalsResponse>
{
    private readonly IConversationRepository _conversationRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;

    public GetMyProposalsHandler(
        IConversationRepository conversationRepo,
        ICurrentUserService currentUser,
        IIdentityService identityService)
    {
        _conversationRepo = conversationRepo;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<GetMyProposalsResponse> Handle(GetMyProposalsQuery request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedException("Must be authenticated");

        var userId = _currentUser.UserId ?? throw new UnauthorizedException();

        // Lấy tất cả proposals mà user là member
        var proposals = await _conversationRepo.GetQueryableSet()
            .Include(c => c.Members)
            .Include(c => c.Subject)
            .Include(c => c.ConversationTags).ThenInclude(t => t.Tag)
            .Where(c => !c.IsDeleted
                && c.ConversationStatus == ConversationStatus.Proposed
                && c.Members.Any(m => m.UserId == userId && !m.IsDeleted))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

        var proposalDtos = new List<ProposalDto>();

        foreach (var conv in proposals)
        {
            var myMember = conv.Members.First(m => m.UserId == userId && !m.IsDeleted);
            var activeMembers = conv.Members.Where(m => !m.IsDeleted).ToList();

            var memberDtos = new List<ProposalMemberDto>();
            foreach (var member in activeMembers)
            {
                var user = await _identityService.FindByIdAsync(member.UserId);
                memberDtos.Add(new ProposalMemberDto
                {
                    UserId = member.UserId,
                    FullName = user?.FullName ?? "Unknown",
                    AvatarUrl = user?.AvatarUrl,
                    Status = member.InviteStatus,
                    SimilarityScore = member.SimilarityScore,
                    RespondedAt = member.RespondedAt
                });
            }

            proposalDtos.Add(new ProposalDto
            {
                ConversationId = conv.Id,
                ConversationName = conv.ConversationName,
                SubjectName = conv.Subject?.SubjectName,
                Tags = conv.ConversationTags.Select(t => t.Tag.TagName).ToList(),
                AvatarUrl = conv.AvatarUrl,
                TotalMembers = activeMembers.Count,
                AcceptedCount = activeMembers.Count(m => m.InviteStatus == MemberInviteStatus.Accepted),
                PendingCount = activeMembers.Count(m => m.InviteStatus == MemberInviteStatus.Pending),
                DeclinedCount = activeMembers.Count(m => m.InviteStatus == MemberInviteStatus.Declined),
                MyStatus = myMember.InviteStatus,
                MySimilarityScore = myMember.SimilarityScore,
                ExpiresAt = conv.ProposalExpiresAt,
                CreatedAt = conv.CreatedAt,
                Members = memberDtos
            });
        }

        return new GetMyProposalsResponse { Proposals = proposalDtos };
    }
}

