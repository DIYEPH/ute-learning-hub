using MediatR;
using Microsoft.Extensions.Logging;
using UteLearningHub.Application.Events;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.EventHandlers;

public class SendAchievementNotificationHandler : INotificationHandler<TrustLevelChangedEvent>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<SendAchievementNotificationHandler> _logger;

    public SendAchievementNotificationHandler(
        INotificationRepository notificationRepository,
        IDateTimeProvider dateTimeProvider,
        ILogger<SendAchievementNotificationHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task Handle(TrustLevelChangedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var (title, content) = GetAchievementMessage(notification.NewLevel);

            if (title == null || content == null)
                return;

            var notificationEntity = new Notification
            {
                Id = Guid.NewGuid(),
                ObjectId = notification.UserId,
                Title = title,
                Content = content,
                Link = "/profile",
                IsGlobal = false,
                NotificationType = NotificationType.System,
                NotificationPriorityType = NotificationPriorityType.Hight,
                ExpiredAt = _dateTimeProvider.OffsetNow.AddDays(30),
                CreatedById = notification.UserId,
                CreatedAt = _dateTimeProvider.OffsetNow
            };

            var recipient = new NotificationRecipient
            {
                Id = Guid.NewGuid(),
                NotificationId = notificationEntity.Id,
                RecipientId = notification.UserId,
                IsRead = false,
                ReceivedAt = _dateTimeProvider.OffsetNow
            };

            notificationEntity.Recipients = new List<NotificationRecipient> { recipient };

            _notificationRepository.Add(notificationEntity);
            await _notificationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Sent achievement notification to UserId: {UserId} for reaching {NewLevel}",
                notification.UserId,
                notification.NewLevel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send achievement notification to UserId: {UserId}",
                notification.UserId);
            // Don't throw - notification failure shouldn't break the main flow
        }
    }

    private static (string? Title, string? Content) GetAchievementMessage(TrustLever level)
    {
        return level switch
        {
            TrustLever.Newbie => (
                "🎉 Chúc mừng! Bạn đạt danh hiệu Người Mới",
                "Chào mừng bạn đến với cộng đồng! Tiếp tục đóng góp để nâng cao danh hiệu nhé."
            ),
            TrustLever.Contributor => (
                "⭐ Chúc mừng! Bạn đạt danh hiệu Người Đóng Góp",
                "Bạn đã có những đóng góp tích cực! Hãy tiếp tục chia sẻ kiến thức."
            ),
            TrustLever.TrustedMember => (
                "🏆 Chúc mừng! Bạn đạt danh hiệu Thành Viên Tin Cậy",
                "Bạn là thành viên được tin tưởng! Tài liệu của bạn sẽ được tự động phê duyệt."
            ),
            TrustLever.Moderator => (
                "👑 Chúc mừng! Bạn đạt danh hiệu Kiểm Duyệt Viên",
                "Bạn giờ có quyền kiểm duyệt tài liệu và bình luận của người khác!"
            ),
            TrustLever.Master => (
                "💎 Chúc mừng! Bạn đạt danh hiệu Bậc Thầy",
                "Bạn là một trong những thành viên xuất sắc nhất! Cảm ơn vì những đóng góp to lớn."
            ),
            _ => (null, null)
        };
    }
}
