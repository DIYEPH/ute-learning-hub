using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.TrustScore;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using ReportEntity = UteLearningHub.Domain.Entities.Report;
using DocumentFileEntity = UteLearningHub.Domain.Entities.DocumentFile;
using CommentEntity = UteLearningHub.Domain.Entities.Comment;
using NotificationEntity = UteLearningHub.Domain.Entities.Notification;

namespace UteLearningHub.Application.Features.Report.Commands.ReviewReport;


public class ReviewReportCommandHandler : IRequestHandler<ReviewReportCommand, Unit>
{
    private readonly IReportRepository _reportRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly ITrustScoreService _trustScoreService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReviewReportCommandHandler(
        IReportRepository reportRepository,
        IDocumentRepository documentRepository,
        ICommentRepository commentRepository,
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        IUserService userService,
        ITrustScoreService trustScoreService,
        IDateTimeProvider dateTimeProvider)
    {
        _reportRepository = reportRepository;
        _documentRepository = documentRepository;
        _commentRepository = commentRepository;
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _userService = userService;
        _trustScoreService = trustScoreService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(ReviewReportCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to review reports");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Only admin or moderator can review reports
        var isAdmin = _currentUserService.IsInRole("Admin");
        var trustLevel = await _userService.GetTrustLevelAsync(userId, cancellationToken);

        if (!isAdmin && (!trustLevel.HasValue || trustLevel.Value < TrustLever.Moderator))
            throw new UnauthorizedException("Only administrators or moderators can review reports");

        // Get report with Document/Comment for reward calculation
        var report = await _reportRepository.GetByIdWithContentAsync(request.ReportId, cancellationToken);
        if (report == null || report.IsDeleted)
            throw new NotFoundException($"Report with id {request.ReportId} not found");

        var oldStatus = report.Status;
        var now = _dateTimeProvider.OffsetNow;

        // Update current report
        report.Status = request.Status;
        report.ReviewedById = userId;
        report.ReviewedAt = now;
        report.ReviewNote = request.ReviewNote;

        await _reportRepository.UpdateAsync(report, cancellationToken);

        // If approving, auto-approve related reports and reward/notify reporters
        if (request.Status == ContentStatus.Approved && oldStatus != ContentStatus.Approved)
        {
            var relatedReports = await _reportRepository.GetRelatedPendingReportsAsync(
                report.DocumentFileId, report.CommentId, cancellationToken);

            // Collect all reports (current + related)
            var allReports = new List<ReportEntity> { report };
            
            foreach (var related in relatedReports.Where(r => r.Id != report.Id))
            {
                related.Status = ContentStatus.Approved;
                related.ReviewedById = userId;
                related.ReviewedAt = now;
                related.ReviewNote = "Auto-approved (cùng nội dung vi phạm)";
                allReports.Add(related);
            }

            // Hide the reported content (set Status = Hidden)
            if (report.DocumentFileId.HasValue)
            {
                var documentFile = await _documentRepository.GetDocumentFileByIdAsync(
                    report.DocumentFileId.Value, disableTracking: false, cancellationToken);
                if (documentFile != null)
                {
                    documentFile.Status = ContentStatus.Hidden;
                }
            }
            
            if (report.CommentId.HasValue)
            {
                var comment = await _commentRepository.GetByIdAsync(
                    report.CommentId.Value, disableTracking: false, cancellationToken);
                if (comment != null)
                {
                    comment.Status = ContentStatus.Hidden;
                }
            }


            await _reportRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            // Process rewards and notifications (async)
            _ = ProcessRewardsAndNotificationsAsync(allReports, report.DocumentFile, report.Comment, now, cancellationToken);
        }

        else
        {
            await _reportRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }

    private async Task ProcessRewardsAndNotificationsAsync(
        List<ReportEntity> reports,
        DocumentFileEntity? documentFile,
        CommentEntity? comment,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        try
        {
            var contentCreatedAt = documentFile?.CreatedAt ?? comment?.CreatedAt ?? DateTimeOffset.UtcNow;
            var contentName = documentFile?.Title ?? "bình luận";

            // Sort by report time and separate rewarded vs late reporters
            var sortedReports = reports.OrderBy(r => r.CreatedAt).ToList();
            var rewardedReports = sortedReports.Take(TrustScoreConstants.MaxRewardedReporters).ToList();
            var lateReports = sortedReports.Skip(TrustScoreConstants.MaxRewardedReporters).ToList();

            // 1. Notify content owner to fix their content
            var contentOwnerId = documentFile?.CreatedById ?? comment?.CreatedById;
            if (contentOwnerId.HasValue)
            {
                await CreateNotificationAsync(
                    contentOwnerId.Value,
                    "⚠️ Nội dung của bạn bị báo cáo vi phạm",
                    $"Nội dung \"{contentName}\" đã bị báo cáo và xác nhận vi phạm. Vui lòng kiểm tra và chỉnh sửa.",
                    documentFile != null ? $"/documents/{documentFile.DocumentId}" : null,
                    now,
                    cancellationToken);
            }

            // 2. Reward and notify TOP N reporters
            foreach (var report in rewardedReports)
            {
                var points = TrustScoreConstants.CalculateReportRewardPoints(contentCreatedAt, report.CreatedAt);
                
                await _trustScoreService.AddTrustScoreAsync(
                    report.CreatedById,
                    points,
                    $"Báo cáo được duyệt (+{points}đ)",
                    null,
                    cancellationToken);

                await CreateNotificationAsync(
                    report.CreatedById,
                    "🎉 Báo cáo của bạn được duyệt!",
                    $"Cảm ơn bạn đã báo cáo vi phạm! Bạn được thưởng +{points} điểm uy tín.",
                    "/profile",
                    now,
                    cancellationToken);
            }

            // 3. Notify late reporters (no points, but appreciated)
            foreach (var report in lateReports)
            {
                await CreateNotificationAsync(
                    report.CreatedById,
                    "📋 Báo cáo được ghi nhận",
                    "Cảm ơn bạn đã báo cáo! Rất tiếc, đã có nhiều người báo cáo trước bạn nên bạn không nhận được điểm thưởng lần này.",
                    null,
                    now,
                    cancellationToken);
            }
        }
        catch
        {
            // Log error nhưng không throw
        }
    }

    private async Task CreateNotificationAsync(
        Guid recipientId,
        string title,
        string content,
        string? link,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var notification = new NotificationEntity
        {
            Id = Guid.NewGuid(),
            Title = title,
            Content = content,
            Link = link,
            IsGlobal = false,
            NotificationType = NotificationType.System,
            NotificationPriorityType = NotificationPriorityType.Normal,
            CreatedAt = now
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _notificationRepository.CreateNotificationRecipientsAsync(
            notification.Id,
            new[] { recipientId },
            now,
            cancellationToken);
        await _notificationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
