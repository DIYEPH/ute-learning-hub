using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetMyProposals;

public class GetMyProposalsHandler(
    IConversationRepository conversationRepo,
    ICurrentUserService currentUser,
    IIdentityService identityService) : IRequestHandler<GetMyProposalsQuery, GetMyProposalsResponse>
{
    public async Task<GetMyProposalsResponse> Handle(GetMyProposalsQuery request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedException("Must be authenticated");

        var userId = currentUser.UserId ?? throw new UnauthorizedException();

        var now = DateTimeOffset.UtcNow;

        // Lấy proposals chưa hết hạn mà user là member
        var proposals = await conversationRepo.GetQueryableSet()
            .Include(c => c.Members)
            .Include(c => c.Subject)
            .Include(c => c.ConversationTags).ThenInclude(t => t.Tag)
            .Where(c => c.ConversationStatus == ConversationStatus.Proposed
                && (!c.ProposalExpiresAt.HasValue || c.ProposalExpiresAt.Value > now)
                && c.Members.Any(m => m.UserId == userId && !m.IsDeleted))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

        // Load users theo batch
        var allMemberUserIds = proposals
            .SelectMany(c => c.Members.Where(m => !m.IsDeleted).Select(m => m.UserId))
            .Distinct()
            .ToList();

        var userInfoDict = await identityService.FindByIdsAsync(allMemberUserIds, ct);
        var userInfo = userInfoDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var proposalDtos = new List<ProposalDto>();

        foreach (var conv in proposals)
        {
            var myMember = conv.Members.First(m => m.UserId == userId && !m.IsDeleted);
            var activeMembers = conv.Members.Where(m => !m.IsDeleted).ToList();

            var memberDtos = activeMembers.Select(member =>
            {
                var user = userInfo.TryGetValue(member.UserId, out var u) ? u : null;
                return new ProposalMemberDto
                {
                    UserId = member.UserId,
                    FullName = user?.FullName ?? "Unknown",
                    AvatarUrl = user?.AvatarUrl,
                    Status = member.InviteStatus,
                    SimilarityScore = member.SimilarityScore,
                    RespondedAt = member.RespondedAt
                };
            }).ToList();

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