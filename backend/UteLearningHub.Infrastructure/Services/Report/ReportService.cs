using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Document.Commands.ReviewDocumentFile;
using UteLearningHub.Application.Features.Report.Commands.CreateReport;
using UteLearningHub.Application.Features.Report.Commands.ReviewReport;
using UteLearningHub.Application.Services.Comment;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Report;
using UteLearningHub.Application.Services.TrustScore;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using CommentEntity = UteLearningHub.Domain.Entities.Comment;
using DocumentFileEntity = UteLearningHub.Domain.Entities.DocumentFile;
using NotificationEntity = UteLearningHub.Domain.Entities.Notification;
using ReportEntity = UteLearningHub.Domain.Entities.Report;

namespace UteLearningHub.Infrastructure.Services.Report;

public class ReportService(
    IReportRepository reportRepository,
    IDocumentRepository documentRepository,
    ICommentRepository commentRepository,
    INotificationRepository notificationRepository,
    ICurrentUserService currentUserService,
    ICommentService commentService,
    IDateTimeProvider dateTimeProvider,
    IUserService userService,
    ITrustScoreService trustScoreService,
    IMediator mediator) : IReportService
{
    public async Task<ReportDto> CreateAsync(CreateReportCommand request, CancellationToken ct = default)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create reports");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();
        var now = dateTimeProvider.OffsetNow;
        var isAdmin = currentUserService.IsInRole("Admin");
        var trustLevel = await userService.GetTrustLevelAsync(userId, ct);

        if (!isAdmin && (!trustLevel.HasValue || trustLevel.Value < TrustLever.Newbie))
            throw new UnauthorizedException("Only administrators or users with Newbie+ trust level can create reports");

        if (!request.DocumentFileId.HasValue && !request.CommentId.HasValue)
            throw new BadRequestException("Either DocumentFileId or CommentId must be provided");
        if (request.DocumentFileId.HasValue && request.CommentId.HasValue)
            throw new BadRequestException("Cannot report both document file and comment at the same time");

        // Validate document file or comment exists
        if (request.DocumentFileId.HasValue)
        {
            var docFile = await documentRepository.GetDocumentFileByIdAsync(request.DocumentFileId.Value, true, ct);
            if (docFile == null || docFile.IsDeleted)
                throw new NotFoundException($"DocumentFile with id {request.DocumentFileId.Value} not found");
        }
        if (request.CommentId.HasValue)
        {
            var comment = await commentRepository.GetByIdAsync(request.CommentId.Value, true, ct);
            if (comment == null || comment.IsDeleted)
                throw new NotFoundException($"Comment with id {request.CommentId.Value} not found");
        }

        // Check if user already has a pending report
        var existingPendingReport = await reportRepository.GetUserPendingReportAsync(
            userId, request.DocumentFileId, request.CommentId, ct);
        if (existingPendingReport != null)
            throw new BadRequestException("Bạn đã có báo cáo đang chờ duyệt về nội dung này");

        var isTrustedMemberOrHigher = isAdmin || (trustLevel.HasValue && trustLevel.Value >= TrustLever.TrustedMember);

        // Check daily instant approve limit
        var canInstantApprove = isTrustedMemberOrHigher;
        if (canInstantApprove && !isAdmin)
        {
            var dailyCount = await reportRepository.GetDailyInstantApproveCountAsync(userId, now, ct);
            if (dailyCount >= TrustScoreConstants.TrustedMemberDailyReportLimit)
                canInstantApprove = false;
        }

        var report = new ReportEntity
        {
            Id = Guid.NewGuid(),
            DocumentFileId = request.DocumentFileId,
            CommentId = request.CommentId,
            Reason = request.Reason,
            Content = request.Content ?? string.Empty,
            Status = canInstantApprove ? ContentStatus.Approved : ContentStatus.PendingReview,
            CreatedById = userId,
            CreatedAt = now,
            ReviewedById = canInstantApprove ? userId : null,
            ReviewedAt = canInstantApprove ? now : null,
            ReviewNote = canInstantApprove ? $"{request.Content}, Tự động xử lý thành viên uy tín: {userId}" : null
        };

        reportRepository.Add(report);
        await reportRepository.UnitOfWork.SaveChangesAsync(ct);

        if (canInstantApprove)
            await AutoProcessReportAsync(report, userId, now, ct);

        var authorInfo = await commentService.GetCommentAuthorsAsync([userId], ct);
        var reporter = authorInfo.GetValueOrDefault(userId);

        return new ReportDto
        {
            Id = report.Id,
            DocumentFileId = report.DocumentFileId,
            CommentId = report.CommentId,
            Reason = report.Reason,
            Content = report.Content,
            ReporterName = reporter?.FullName ?? "Unknown",
            ReporterAvatarUrl = reporter?.AvatarUrl,
            CreatedById = report.CreatedById,
            Status = report.Status,
            CreatedAt = report.CreatedAt
        };
    }

    public async Task<ReportDto> ReviewAsync(ReviewReportCommand request, CancellationToken ct = default)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to review reports");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        // Only admin or moderator can review reports
        var isAdmin = currentUserService.IsInRole("Admin");
        var trustLevel = await userService.GetTrustLevelAsync(userId, ct);

        if (!isAdmin && (!trustLevel.HasValue || trustLevel.Value < TrustLever.Moderator))
            throw new UnauthorizedException("Only administrators or moderators can review reports");

        // Get report with Document/Comment for reward calculation
        var report = await reportRepository.GetByIdWithContentAsync(request.ReportId, ct);
        if (report == null)
            throw new NotFoundException($"Report with id {request.ReportId} not found");

        var oldStatus = report.Status;
        var now = dateTimeProvider.OffsetNow;

        // Update current report
        report.Status = request.Status;
        report.ReviewedById = userId;
        report.ReviewedAt = now;
        report.ReviewNote = request.ReviewNote;

        reportRepository.Update(report);

        // Nếu duyệt, tự động duyệt các báo cáo liên quan và thưởng/thông báo cho người báo cáo
        if (request.Status == ContentStatus.Approved && oldStatus != ContentStatus.Approved)
        {
            // Lấy các báo cáo cùng lý do để thưởng (chỉ cùng lý do mới được cộng điểm)
            var sameReasonReports = await reportRepository.GetRelatedPendingReportsAsync(
                report.DocumentFileId, report.CommentId, report.Reason, ct);

            var rewardedReports = new List<ReportEntity> { report };

            foreach (var related in sameReasonReports.Where(r => r.Id != report.Id))
            {
                related.Status = ContentStatus.Approved;
                related.ReviewedById = userId;
                related.ReviewedAt = now;
                related.ReviewNote = "Tự động duyệt (cùng loại báo cáo)";
                rewardedReports.Add(related);
            }

            // Note: Reports with different reasons remain pending for separate review
            // Each report reason should be evaluated independently by admin

            // Hide the reported content
            if (report.DocumentFileId.HasValue)
            {
                await mediator.Send(new ReviewDocumentFileCommand
                {
                    DocumentFileId = report.DocumentFileId.Value,
                    Status = ContentStatus.Hidden,
                    Reason = request.ReviewNote ?? "Bị ẩn do báo cáo vi phạm"
                }, ct);
            }

            if (report.CommentId.HasValue)
            {
                var comment = await commentRepository.GetByIdAsync(report.CommentId.Value, disableTracking: false, ct);
                if (comment != null)
                {
                    comment.Status = ContentStatus.Hidden;
                    comment.ReviewedById = userId;
                    comment.ReviewedAt = now;
                    comment.ReviewNote = request.ReviewNote ?? "Bị ẩn do báo cáo vi phạm";
                }
            }

            await reportRepository.UnitOfWork.SaveChangesAsync(ct);

            //  Xử lý thưởng và thông báo chỉ cho những người báo cáo cùng lý do
            _ = ProcessRewardsAndNotificationsAsync(rewardedReports, report.DocumentFile, report.Comment, userId, now, ct);
        }
        else if (request.Status == ContentStatus.Hidden) // Report rejected
        {
            await reportRepository.UnitOfWork.SaveChangesAsync(ct);

            _ = CreateNotificationAsync(
                report.CreatedById,
                "Báo cáo không được chấp nhận",
                request.ReviewNote ?? "Báo cáo của bạn đã được xem xét và không được chấp nhận.",
                null,
                now,
                ct);
        }
        else
        {
            await reportRepository.UnitOfWork.SaveChangesAsync(ct);
        }

        var authorInfo = await commentService.GetCommentAuthorsAsync([report.CreatedById], ct);
        var reporter = authorInfo.GetValueOrDefault(report.CreatedById);

        return new ReportDto
        {
            Id = report.Id,
            DocumentFileId = report.DocumentFileId,
            CommentId = report.CommentId,
            Reason = report.Reason,
            Content = report.Content,
            ReporterName = reporter?.FullName ?? "Unknown",
            ReporterAvatarUrl = reporter?.AvatarUrl,
            CreatedById = report.CreatedById,
            Status = report.Status,
            CreatedAt = report.CreatedAt
        };
    }

    private async Task AutoProcessReportAsync(ReportEntity report, Guid userId, DateTimeOffset now, CancellationToken ct)
    {
        // 1. Hide the reported content
        if (report.DocumentFileId.HasValue)
        {
            await mediator.Send(new ReviewDocumentFileCommand
            {
                DocumentFileId = report.DocumentFileId.Value,
                Status = ContentStatus.Hidden,
                Reason = report.ReviewNote ?? "Bị ẩn do báo cáo vi phạm"
            }, ct);
        }

        if (report.CommentId.HasValue)
        {
            var comment = await commentRepository.GetByIdAsync(report.CommentId.Value, false, ct);
            if (comment != null)
            {
                comment.Status = ContentStatus.Hidden;
                comment.ReviewedById = userId;
                comment.ReviewedAt = now;
                comment.ReviewNote = report.ReviewNote ?? "Bị ẩn do báo cáo vi phạm";
                await commentRepository.UnitOfWork.SaveChangesAsync(ct);
            }
        }

        // 2. Get reports with same reason for rewarding (only same reason gets points)
        var sameReasonReports = await reportRepository.GetRelatedPendingReportsAsync(
            report.DocumentFileId, report.CommentId, report.Reason, ct);
        var rewardedReports = new List<ReportEntity> { report };
        foreach (var related in sameReasonReports.Where(r => r.Id != report.Id))
        {
            related.Status = ContentStatus.Approved;
            related.ReviewedById = userId;
            related.ReviewedAt = now;
            related.ReviewNote = "Tự động duyệt (cùng loại báo cáo)";
            rewardedReports.Add(related);
        }

        // Note: Reports with different reasons remain pending for separate review
        // Each report reason should be evaluated independently by admin

        await reportRepository.UnitOfWork.SaveChangesAsync(ct);

        // 3. Notify and reward only reporters with same reason
        _ = ProcessRewardsAndNotificationsAsync(rewardedReports, null, null, userId, now, ct);
    }

    private async Task ProcessRewardsAndNotificationsAsync(
        List<ReportEntity> reports,
        DocumentFileEntity? documentFile,
        CommentEntity? comment,
        Guid reviewerId,
        DateTimeOffset now,
        CancellationToken ct)
    {
        try
        {
            string contentName = "nội dung";
            Guid? contentOwnerId = null;
            DateTimeOffset contentCreatedAt = now;

            if (documentFile != null)
            {
                contentName = documentFile.Title ?? "tài liệu";
                contentOwnerId = documentFile.CreatedById;
                contentCreatedAt = documentFile.CreatedAt;
            }
            else if (comment != null)
            {
                contentName = "bình luận";
                contentOwnerId = comment.CreatedById;
                contentCreatedAt = comment.CreatedAt;
            }
            else if (reports.FirstOrDefault() is { } firstReport)
            {
                if (firstReport.DocumentFileId.HasValue)
                {
                    var docFile = await documentRepository.GetDocumentFileByIdAsync(firstReport.DocumentFileId.Value, true, ct);
                    if (docFile != null)
                    {
                        contentName = docFile.Title ?? "tài liệu";
                        contentOwnerId = docFile.CreatedById;
                        contentCreatedAt = docFile.CreatedAt;
                    }
                }
                else if (firstReport.CommentId.HasValue)
                {
                    var cmt = await commentRepository.GetByIdAsync(firstReport.CommentId.Value, true, ct);
                    if (cmt != null)
                    {
                        contentName = "bình luận";
                        contentOwnerId = cmt.CreatedById;
                        contentCreatedAt = cmt.CreatedAt;
                    }
                }
            }

            // Notify content owner
            if (contentOwnerId.HasValue)
            {
                await CreateNotificationAsync(
                    contentOwnerId.Value,
                    "Nội dung của bạn bị báo cáo vi phạm",
                    $"Nội dung \"{contentName}\" đã bị báo cáo và xác nhận vi phạm. Vui lòng kiểm tra và chỉnh sửa.",
                    null,
                    now,
                    ct);
            }

            // Reward and notify reporters
            var sortedReports = reports.OrderBy(r => r.CreatedAt).ToList();
            var rewardedReports = sortedReports.Take(TrustScoreConstants.MaxRewardedReporters).ToList();
            var lateReports = sortedReports.Skip(TrustScoreConstants.MaxRewardedReporters).ToList();

            foreach (var report in rewardedReports)
            {
                if (report.CreatedById == reviewerId)
                {
                    await CreateNotificationAsync(report.CreatedById, "Báo cáo được duyệt", "Báo cáo của bạn đã được xử lý. Không nhận điểm vì tự duyệt.", null, now, ct);
                    continue;
                }

                var points = TrustScoreConstants.CalculateReportRewardPoints(contentCreatedAt, report.CreatedAt);
                await trustScoreService.AddTrustScoreAsync(report.CreatedById, points, $"Báo cáo được duyệt (+{points}đ)", report.Id, TrustEntityType.Report, ct);
                await CreateNotificationAsync(report.CreatedById, "Báo cáo của bạn được duyệt!", $"Cảm ơn bạn đã báo cáo vi phạm! Bạn được thưởng +{points} điểm uy tín.", "/profile", now, ct);
            }

            foreach (var report in lateReports)
            {
                await CreateNotificationAsync(report.CreatedById, "Báo cáo được ghi nhận", "Cảm ơn bạn đã báo cáo! Rất tiếc, đã có nhiều người báo cáo trước bạn nên bạn không nhận được điểm thưởng lần này.", null, now, ct);
            }
        }
        catch { /* Log error but don't throw */ }
    }

    private async Task CreateNotificationAsync(Guid recipientId, string title, string content, string? link, DateTimeOffset now, CancellationToken ct)
    {
        var notification = new NotificationEntity
        {
            Id = Guid.NewGuid(),
            Title = title,
            Content = content,
            Link = link ?? string.Empty,
            IsGlobal = false,
            NotificationType = NotificationType.System,
            NotificationPriorityType = NotificationPriorityType.Normal,
            CreatedAt = now
        };
        notificationRepository.Add(notification);
        await notificationRepository.CreateNotificationRecipientsAsync(notification.Id, [recipientId], now, ct);
        await notificationRepository.UnitOfWork.SaveChangesAsync(ct);
    }
}
