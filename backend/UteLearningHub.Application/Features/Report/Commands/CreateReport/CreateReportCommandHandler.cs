using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.DocumentFiles.Commands.ReviewDocumentFile;
using UteLearningHub.Application.Services.Comment;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.TrustScore;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using ReportEntity = UteLearningHub.Domain.Entities.Report;
using NotificationEntity = UteLearningHub.Domain.Entities.Notification;

namespace UteLearningHub.Application.Features.Report.Commands.CreateReport;

public class CreateReportCommandHandler(
    IReportRepository reportRepository,
    IDocumentRepository documentRepository,
    ICommentRepository commentRepository,
    INotificationRepository notificationRepository,
    ICurrentUserService currentUserService,
    ICommentService commentService,
    IDateTimeProvider dateTimeProvider,
    IUserService userService,
    ITrustScoreService trustScoreService,
    IMediator mediator) : IRequestHandler<CreateReportCommand, ReportDto>
{
    public async Task<ReportDto> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create reports");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();
        var now = dateTimeProvider.OffsetNow;
        var isAdmin = currentUserService.IsInRole("Admin");
        var trustLevel = await userService.GetTrustLevelAsync(userId, cancellationToken);

        if (!isAdmin && (!trustLevel.HasValue || trustLevel.Value < TrustLever.Newbie))
            throw new UnauthorizedException("Only administrators or users with Newbie+ trust level can create reports");

        // Validate: must have either DocumentFileId or CommentId, but not both
        if (!request.DocumentFileId.HasValue && !request.CommentId.HasValue)
            throw new BadRequestException("Either DocumentFileId or CommentId must be provided");
        if (request.DocumentFileId.HasValue && request.CommentId.HasValue)
            throw new BadRequestException("Cannot report both document file and comment at the same time");

        // Validate document file or comment exists
        if (request.DocumentFileId.HasValue)
        {
            var docFile = await documentRepository.GetDocumentFileByIdAsync(request.DocumentFileId.Value, true, cancellationToken);
            if (docFile == null || docFile.IsDeleted)
                throw new NotFoundException($"DocumentFile with id {request.DocumentFileId.Value} not found");
        }
        if (request.CommentId.HasValue)
        {
            var comment = await commentRepository.GetByIdAsync(request.CommentId.Value, true, cancellationToken);
            if (comment == null || comment.IsDeleted)
                throw new NotFoundException($"Comment with id {request.CommentId.Value} not found");
        }

        var isTrustedMemberOrHigher = isAdmin || (trustLevel.HasValue && trustLevel.Value >= TrustLever.TrustedMember);

        var report = new ReportEntity
        {
            Id = Guid.NewGuid(),
            DocumentFileId = request.DocumentFileId,
            CommentId = request.CommentId,
            Content = request.Content,
            Status = isTrustedMemberOrHigher ? ContentStatus.Approved : ContentStatus.PendingReview,
            CreatedById = userId,
            CreatedAt = now,
            ReviewedById = isTrustedMemberOrHigher ? userId : null,
            ReviewedAt = isTrustedMemberOrHigher ? now : null,
            ReviewNote = isTrustedMemberOrHigher ? $"{request.Content} | Tự động xử lý thành viên uy tín" : null
        };

        await reportRepository.AddAsync(report, cancellationToken);
        await reportRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        if (isTrustedMemberOrHigher)
            await AutoProcessReportAsync(report, userId, now, cancellationToken);

        var authorInfo = await commentService.GetCommentAuthorsAsync([userId], cancellationToken);
        var reporter = authorInfo.GetValueOrDefault(userId);

        return new ReportDto
        {
            Id = report.Id,
            DocumentFileId = report.DocumentFileId,
            CommentId = report.CommentId,
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

        // 2. Close all related pending reports
        var relatedReports = await reportRepository.GetRelatedPendingReportsAsync(report.DocumentFileId, report.CommentId, ct);
        var allReports = new List<ReportEntity> { report };
        foreach (var related in relatedReports.Where(r => r.Id != report.Id))
        {
            related.Status = ContentStatus.Approved;
            related.ReviewedById = userId;
            related.ReviewedAt = now;
            related.ReviewNote = "Tự động duyệt (cùng nội dung vi phạm)";
            allReports.Add(related);
        }
        await reportRepository.UnitOfWork.SaveChangesAsync(ct);

        // 3. Notify and reward reporters (async, non-blocking)
        _ = ProcessRewardsAndNotificationsAsync(allReports, report.DocumentFileId, report.CommentId, userId, now, ct);
    }

    private async Task ProcessRewardsAndNotificationsAsync(
        List<ReportEntity> reports, Guid? documentFileId, Guid? commentId, Guid reviewerId, DateTimeOffset now, CancellationToken ct)
    {
        try
        {
            string contentName = "nội dung";
            Guid? contentOwnerId = null;
            DateTimeOffset contentCreatedAt = now;

            if (documentFileId.HasValue)
            {
                var docFile = await documentRepository.GetDocumentFileByIdAsync(documentFileId.Value, true, ct);
                if (docFile != null)
                {
                    contentName = docFile.Title ?? "tài liệu";
                    contentOwnerId = docFile.CreatedById;
                    contentCreatedAt = docFile.CreatedAt;
                }
            }
            if (commentId.HasValue)
            {
                var comment = await commentRepository.GetByIdAsync(commentId.Value, true, ct);
                if (comment != null)
                {
                    contentName = "bình luận";
                    contentOwnerId = comment.CreatedById;
                    contentCreatedAt = comment.CreatedAt;
                }
            }

            // Notify content owner
            if (contentOwnerId.HasValue)
                await CreateNotificationAsync(contentOwnerId.Value, "Nội dung của bạn bị báo cáo vi phạm", $"Nội dung \"{contentName}\" đã bị báo cáo và xác nhận vi phạm. Vui lòng kiểm tra và chỉnh sửa.", null, now, ct);

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
                await CreateNotificationAsync(report.CreatedById, "Báo cáo được ghi nhận", "Cảm ơn bạn đã báo cáo! Rất tiếc, đã có nhiều người báo cáo trước bạn nên bạn không nhận được điểm thưởng lần này.", null, now, ct);
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
            Link = link,
            IsGlobal = false,
            NotificationType = NotificationType.System,
            NotificationPriorityType = NotificationPriorityType.Normal,
            CreatedAt = now
        };
        await notificationRepository.AddAsync(notification, ct);
        await notificationRepository.CreateNotificationRecipientsAsync(notification.Id, [recipientId], now, ct);
        await notificationRepository.UnitOfWork.SaveChangesAsync(ct);
    }
}
