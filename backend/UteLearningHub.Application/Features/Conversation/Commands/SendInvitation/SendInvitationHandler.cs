using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Commands.SendInvitation;

public class SendInvitationHandler : IRequestHandler<SendInvitationCommand, SendInvitationResponse>
{
    private readonly IConversationRepository _convRepo;
    private readonly INotificationRepository _notificationRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;

    public SendInvitationHandler(
        IConversationRepository convRepo,
        INotificationRepository notificationRepo,
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime)
    {
        _convRepo = convRepo;
        _notificationRepo = notificationRepo;
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public async Task<SendInvitationResponse> Handle(SendInvitationCommand req, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedException("Must be authenticated");

        var userId = _currentUser.UserId ?? throw new UnauthorizedException();

        // Kiểm tra conversation
        var conv = await _convRepo.GetByIdWithDetailsAsync(req.ConversationId, true, ct);
        if (conv == null || conv.IsDeleted)
            throw new NotFoundException("Conversation not found");

        // Kiểm tra quyền (phải là admin/owner)
        var member = conv.Members.FirstOrDefault(m => m.UserId == userId && !m.IsDeleted);
        if (member == null)
            throw new ForbiddenException("You are not a member of this conversation");

        if (member.ConversationMemberRoleType != ConversationMemberRoleType.Deputy &&
            member.ConversationMemberRoleType != ConversationMemberRoleType.Owner)
            throw new ForbiddenException("Only deputy or owner can send invitations");

        // Kiểm tra user được mời đã là member chưa
        var isAlreadyMember = conv.Members.Any(m => m.UserId == req.InvitedUserId && !m.IsDeleted);
        if (isAlreadyMember)
            return new SendInvitationResponse { Success = false, Error = "User is already a member" };

        // Kiểm tra đã có lời mời pending chưa
        var existingInvite = await _convRepo.GetInvitationsQueryable()
            .FirstOrDefaultAsync(i => 
                i.ConversationId == req.ConversationId && 
                i.InvitedUserId == req.InvitedUserId && 
                i.Status == ContentStatus.PendingReview && 
                !i.IsDeleted, ct);

        if (existingInvite != null)
            return new SendInvitationResponse { Success = false, Error = "Invitation already sent" };

        // Tạo lời mời
        var invitation = new ConversationInvitation
        {
            Id = Guid.NewGuid(),
            ConversationId = req.ConversationId,
            InvitedUserId = req.InvitedUserId,
            Message = req.Message,
            Status = ContentStatus.PendingReview,
            CreatedById = userId,
            CreatedAt = _dateTime.OffsetNow
        };

        await _convRepo.AddInvitationAsync(invitation, ct);

        // Tạo thông báo cho người được mời
        var notification = new Domain.Entities.Notification
        {
            Id = Guid.NewGuid(),
            ObjectId = invitation.Id,
            Title = "Lời mời tham gia nhóm",
            Content = $"Bạn được mời tham gia nhóm \"{conv.ConversationName}\"",
            Link = $"/conversations",
            IsGlobal = false,
            ExpiredAt = _dateTime.OffsetNow.AddDays(30),
            NotificationType = NotificationType.Conversation,
            NotificationPriorityType = NotificationPriorityType.Normal,
            CreatedById = userId
        };

        await _notificationRepo.AddAsync(notification, ct);
        await _notificationRepo.UnitOfWork.SaveChangesAsync(ct);

        // Tạo recipient cho người được mời
        await _notificationRepo.CreateNotificationRecipientsAsync(
            notification.Id,
            [req.InvitedUserId],
            _dateTime.OffsetNow,
            ct);

        await _convRepo.UnitOfWork.SaveChangesAsync(ct);

        return new SendInvitationResponse
        {
            InvitationId = invitation.Id,
            Success = true
        };
    }
}
