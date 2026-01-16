using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Conversation;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.Conversation;

public class ConversationQueryService(ApplicationDbContext dbContext, IIdentityService identityService) : IConversationQueryService
{
    public async Task<ConversationDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken ct = default)
    {
        var conversation = await dbContext.Conversations
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id, c.ConversationName, c.ConversationType, c.Visibility, c.ConversationStatus,
                c.IsSuggestedByAI, c.IsAllowMemberPin, c.AvatarUrl, c.LastMessage,
                c.CreatedById, c.CreatedAt, c.UpdatedAt,
                Subject = c.Subject != null ? new SubjectDto
                {
                    Id = c.Subject.Id,
                    SubjectName = c.Subject.SubjectName,
                    SubjectCode = c.Subject.SubjectCode
                } : null,
                Tags = c.ConversationTags.Select(ct => new TagDto { Id = ct.Tag.Id, TagName = ct.Tag.TagName }).ToList(),
                Members = c.Members
                    .Where(m => m.InviteStatus == Domain.Constaints.Enums.MemberInviteStatus.Joined)
                    .Select(m => new { m.Id, m.UserId, m.ConversationMemberRoleType, m.IsMuted, m.CreatedAt }).ToList(),
                MessageCount = c.Messages.Count
            })
            .FirstOrDefaultAsync(ct);

        if (conversation == null) return null;

        var memberUserIds = conversation.Members.Select(m => m.UserId).Distinct().ToList();
        var memberInfo = new Dictionary<Guid, (string FullName, string? AvatarUrl)>();

        foreach (var userId in memberUserIds)
        {
            var user = await identityService.FindByIdAsync(userId);
            if (user != null) memberInfo[userId] = (user.FullName, user.AvatarUrl);
        }

        return new ConversationDetailDto
        {
            Id = conversation.Id,
            ConversationName = conversation.ConversationName,
            Tags = conversation.Tags,
            ConversationType = conversation.ConversationType,
            Visibility = conversation.Visibility,
            ConversationStatus = conversation.ConversationStatus,
            IsSuggestedByAI = conversation.IsSuggestedByAI,
            IsAllowMemberPin = conversation.IsAllowMemberPin,
            Subject = conversation.Subject,
            AvatarUrl = conversation.AvatarUrl,
            Members = conversation.Members.Select(m => new ConversationMemberDto
            {
                Id = m.Id,
                UserId = m.UserId,
                UserName = memberInfo.TryGetValue(m.UserId, out var info) ? info.FullName : "Unknown",
                UserAvatarUrl = memberInfo.TryGetValue(m.UserId, out var member) ? member.AvatarUrl : null,
                RoleType = m.ConversationMemberRoleType,
                IsMuted = m.IsMuted,
                JoinedAt = m.CreatedAt
            }).ToList(),
            MessageCount = conversation.MessageCount,
            LastMessageId = conversation.LastMessage,
            CreatedById = conversation.CreatedById,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt
        };
    }
}
