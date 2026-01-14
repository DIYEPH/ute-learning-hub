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

public class RespondToProposalHandler : IRequestHandler<RespondToProposalCommand, RespondToProposalResponse>
{
    private readonly IConversationRepository _conversationRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;
    private readonly INotificationRepository _notificationRepo;

    public RespondToProposalHandler(
        IConversationRepository conversationRepo,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime,
        INotificationRepository notificationRepo)
    {
        _conversationRepo = conversationRepo;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _notificationRepo = notificationRepo;
    }

    public async Task<RespondToProposalResponse> Handle(RespondToProposalCommand request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedException("Must be authenticated");

        var userId = _currentUser.UserId ?? throw new UnauthorizedException();

        // Lấy conversation với members
        var conversation = await _conversationRepo.GetQueryableSet()
            .Include(c => c.Members)
            .Include(c => c.Subject)
            .Include(c => c.ConversationTags).ThenInclude(t => t.Tag)
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId && !c.IsDeleted, ct);

        if (conversation == null)
            throw new NotFoundException("Conversation not found");

        // Kiểm tra đây có phải proposal không
        if (conversation.ConversationStatus != ConversationStatus.Proposed)
            throw new BadRequestException("This conversation is not a proposal");

        // Kiểm tra user có trong danh sách members không
        var member = conversation.Members.FirstOrDefault(m => m.UserId == userId && !m.IsDeleted);
        if (member == null)
            throw new BadRequestException("You are not invited to this proposal");

        // Kiểm tra trạng thái hiện tại
        if (member.InviteStatus != MemberInviteStatus.Pending)
            throw new BadRequestException("You have already responded to this proposal");

        // Cập nhật trạng thái
        var now = _dateTime.OffsetUtcNow;
        member.InviteStatus = request.Accept ? MemberInviteStatus.Accepted : MemberInviteStatus.Declined;
        member.RespondedAt = now;

        bool isActivated = false;
        string message;

        if (request.Accept)
        {
            // Đếm số người đã accept (bao gồm member hiện tại vì đã update status ở trên)
            var acceptedCount = conversation.Members.Count(m =>
                !m.IsDeleted && m.InviteStatus == MemberInviteStatus.Accepted);

            if (acceptedCount >= ProposalSettings.MinMembersToActivate)
            {
                // Kích hoạt nhóm!
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
        else
        {
            message = "Bạn đã từ chối lời mời tham gia nhóm.";
        }

        await _conversationRepo.UnitOfWork.SaveChangesAsync(ct);

        return new RespondToProposalResponse
        {
            Success = true,
            Message = message,
            IsActivated = isActivated,
            Conversation = isActivated ? MapToDto(conversation) : null
        };
    }

    private async Task ActivateConversationAsync(Domain.Entities.Conversation conversation, DateTimeOffset now, CancellationToken ct)
    {
        // 1. Đổi status sang Active
        conversation.ConversationStatus = ConversationStatus.Active;
        conversation.ProposalExpiresAt = null;

        // 2. Lấy danh sách members đã accept, sắp xếp theo thời gian accept
        var acceptedMembers = conversation.Members
            .Where(m => !m.IsDeleted && m.InviteStatus == MemberInviteStatus.Accepted)
            .OrderBy(m => m.RespondedAt)
            .ToList();

        // 3. Người accept đầu tiên trở thành Leader
        for (int i = 0; i < acceptedMembers.Count; i++)
        {
            var member = acceptedMembers[i];
            member.InviteStatus = MemberInviteStatus.Joined;
            member.ConversationMemberRoleType = i == 0 
                ? ConversationMemberRoleType.Owner 
                : ConversationMemberRoleType.Member;
        }

        // 4. Xóa luôn những người Declined (không cần giữ lại)
        var declinedMembers = conversation.Members
            .Where(m => !m.IsDeleted && m.InviteStatus == MemberInviteStatus.Declined)
            .ToList();

        foreach (var member in declinedMembers)
            conversation.Members.Remove(member);
        
        // 5. Giữ lại Pending - họ vẫn có thể join sau khi nhóm được tạo

        // 6. Gửi notification cho tất cả members đã join
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

        _notificationRepo.Add(notification);
        await _notificationRepo.CreateNotificationRecipientsAsync(notification.Id, [userId], now, ct);
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



