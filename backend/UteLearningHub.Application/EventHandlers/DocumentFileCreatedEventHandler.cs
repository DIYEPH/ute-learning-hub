using MediatR;
using Microsoft.Extensions.Logging;
using UteLearningHub.Application.Events;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.EventHandlers;

public class DocumentFileCreatedEventHandler(
    IConversationRepository conversationRepository,
    INotificationRepository notificationRepository,
    IDateTimeProvider dateTimeProvider,
    ILogger<DocumentFileCreatedEventHandler> logger
) : INotificationHandler<DocumentFileCreatedEvent>
{
    public async Task Handle(DocumentFileCreatedEvent e, CancellationToken ct)
    {
        if (!e.SubjectId.HasValue) return;

        var proposal = await conversationRepository.GetProposedBySubjectAsync(e.SubjectId.Value, ct);
        if (proposal == null) return;

        var alreadyMember = proposal.Members.Any(m => m.UserId == e.UserId && !m.IsDeleted);
        if (alreadyMember) return;

        var now = dateTimeProvider.OffsetNow;
        await conversationRepository.AddMemberAsync(new ConversationMember
        {
            Id = Guid.NewGuid(),
            ConversationId = proposal.Id,
            UserId = e.UserId,
            ConversationMemberRoleType = ConversationMemberRoleType.Member,
            InviteStatus = MemberInviteStatus.Pending,
            CreatedAt = now
        }, ct);

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            ObjectId = proposal.Id,
            Title = "AI gợi ý tham gia nhóm học!",
            Content = "Bạn được mời tham gia nhóm học phù hợp với tài liệu bạn vừa đăng.",
            Link = "/conversations",
            IsGlobal = false,
            NotificationType = NotificationType.Conversation,
            NotificationPriorityType = NotificationPriorityType.Normal,
            ExpiredAt = proposal.ProposalExpiresAt ?? now.AddDays(7),
            CreatedAt = now,
            CreatedById = proposal.CreatedById
        };
        notificationRepository.Add(notification);
        await notificationRepository.CreateNotificationRecipientsAsync(notification.Id, new List<Guid> { e.UserId }, now, ct);
        await notificationRepository.UnitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Invited user {UserId} to proposal {ProposalId} for subject {SubjectId}",
            e.UserId, proposal.Id, e.SubjectId);
    }
}
