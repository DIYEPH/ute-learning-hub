using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Policies;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Commands.RespondToProposal;

public class RespondToProposalHandler(
    IConversationRepository conversationRepo,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTime,
    INotificationRepository notificationRepo) : IRequestHandler<RespondToProposalCommand, RespondToProposalResponse>
{
    public async Task<RespondToProposalResponse> Handle(RespondToProposalCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedException("Must be authenticated");

        var userId = currentUser.UserId ?? throw new UnauthorizedException();

        var conversation = await conversationRepo.GetQueryableSet()
            .Include(c => c.Members)
            .Include(c => c.Subject)
            .Include(c => c.ConversationTags).ThenInclude(t => t.Tag)
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId, ct);

        if (conversation == null)
            throw new NotFoundException("Conversation not found");

        // Cho phép respond Actived
        var isProposed = conversation.ConversationStatus == ConversationStatus.Proposed;
        var isActive = conversation.ConversationStatus == ConversationStatus.Active;
        if (!isProposed && !isActive)
            throw new BadRequestException("This conversation is not available for joining");

        // Check hết hạn (chỉ với Proposed)
        if (isProposed && conversation.ProposalExpiresAt.HasValue && conversation.ProposalExpiresAt.Value < dateTime.OffsetUtcNow)
            throw new BadRequestException("Lời mời đã hết hạn");

        var member = conversation.Members.FirstOrDefault(m => m.UserId == userId && !m.IsDeleted);
        if (member == null)
            throw new BadRequestException("You are not invited to this proposal");

        if (member.InviteStatus != MemberInviteStatus.Pending)
            throw new BadRequestException("You have already responded to this proposal");

        var now = dateTime.OffsetUtcNow;
        member.RespondedAt = now;

        bool isActivated = false;
        string message;

        if (request.Accept)
        {
            // Nếu nhóm đã active, chỉ cần join trực tiếp
            if (isActive)
            {
                member.InviteStatus = MemberInviteStatus.Joined;
                member.ConversationMemberRoleType = ConversationMemberRoleType.Member;
                message = "Bạn đã tham gia nhóm thành công!";
            }
            else
            {
                // Nhóm đang Proposed: đếm số người đã accept
                member.InviteStatus = MemberInviteStatus.Accepted;
                var acceptedCount = conversation.Members.Count(m => !m.IsDeleted && m.InviteStatus == MemberInviteStatus.Accepted);

                if (acceptedCount >= ProposalSettings.MinMembersToActivate)
                {
                    await ActivateConversationAsync(conversation, now, ct);
                    isActivated = true;
                    message = "Nhóm đã được kích hoạt thành công!";
                }
                else
                {
                    var remaining = ProposalSettings.MinMembersToActivate - acceptedCount;
                    message = $"Bạn đã đồng ý tham gia. Còn {remaining} người nữa để nhóm được kích hoạt.";
                }
            }
        }
        else
        {
            member.InviteStatus = MemberInviteStatus.Declined;
            message = "Bạn đã từ chối lời mời tham gia nhóm.";
        }

        await conversationRepo.UnitOfWork.SaveChangesAsync(ct);

        return new RespondToProposalResponse
        {
            Success = true,
            Message = message,
            IsActivated = isActivated,
            Conversation = (isActivated || isActive) ? MapToDto(conversation) : null
        };
    }

    private async Task ActivateConversationAsync(Domain.Entities.Conversation conversation, DateTimeOffset now, CancellationToken ct)
    {
        conversation.ConversationStatus = ConversationStatus.Active;
        conversation.ProposalExpiresAt = null;

        // Lấy members đã accept, sắp xếp theo thời gian
        var acceptedMembers = conversation.Members
            .Where(m => !m.IsDeleted && m.InviteStatus == MemberInviteStatus.Accepted)
            .OrderBy(m => m.RespondedAt)
            .ToList();

        // Người accept đầu tiên trở thành Owner
        for (int i = 0; i < acceptedMembers.Count; i++)
        {
            var member = acceptedMembers[i];
            member.InviteStatus = MemberInviteStatus.Joined;
            member.ConversationMemberRoleType = i == 0
                ? ConversationMemberRoleType.Owner
                : ConversationMemberRoleType.Member;
        }

        // Xóa members đã declined
        var declinedMembers = conversation.Members
            .Where(m => !m.IsDeleted && m.InviteStatus == MemberInviteStatus.Declined)
            .ToList();

        foreach (var member in declinedMembers)
            conversation.Members.Remove(member);

        // Gửi notification cho members đã join
        foreach (var member in acceptedMembers)
            await SendGroupActivatedNotificationAsync(member.UserId, conversation, now, ct);
    }

    private async Task SendGroupActivatedNotificationAsync(Guid userId, Domain.Entities.Conversation conversation, DateTimeOffset now, CancellationToken ct)
    {
        var notification = new Domain.Entities.Notification
        {
            Id = Guid.NewGuid(),
            Title = "Nhóm học đã được tạo!",
            Content = $"Nhóm \"{conversation.ConversationName}\" đã đủ người và được kích hoạt. Hãy bắt đầu thảo luận!",
            Link = $"/chat/{conversation.Id}",
            IsGlobal = false,
            NotificationType = NotificationType.Conversation,
            NotificationPriorityType = NotificationPriorityType.Normal,
            ExpiredAt = now.AddDays(30),
            CreatedAt = now,
            CreatedById = conversation.CreatedById
        };

        notificationRepo.Add(notification);
        await notificationRepo.CreateNotificationRecipientsAsync(notification.Id, [userId], now, ct);
    }

    private static ConversationDto MapToDto(Domain.Entities.Conversation c)
    {
        return new ConversationDto
        {
            Id = c.Id,
            ConversationName = c.ConversationName,
            Tags = c.ConversationTags.Select(t => new TagDto { Id = t.Tag.Id, TagName = t.Tag.TagName }).ToList(),
            ConversationType = c.ConversationType,
            Visibility = c.Visibility,
            ConversationStatus = c.ConversationStatus,
            IsSuggestedByAI = c.IsSuggestedByAI,
            IsAllowMemberPin = c.IsAllowMemberPin,
            Subject = c.Subject != null ? new SubjectDto { Id = c.Subject.Id, SubjectName = c.Subject.SubjectName, SubjectCode = c.Subject.SubjectCode } : null,
            AvatarUrl = c.AvatarUrl,
            MemberCount = c.Members.Count(m => !m.IsDeleted && m.InviteStatus == MemberInviteStatus.Joined),
            CreatedById = c.CreatedById,
            CreatedAt = c.CreatedAt
        };
    }
}



