using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Comment;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.TrustScore;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using ReportDomain = UteLearningHub.Domain.Entities.Report;

namespace UteLearningHub.Application.Features.Report.Commands.CreateReport;

public class CreateReportCommandHandler : IRequestHandler<CreateReportCommand, ReportDto>
{
    private readonly IReportRepository _reportRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentService _commentService;
    private readonly ITrustScoreService _trustScoreService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateReportCommandHandler(
        IReportRepository reportRepository,
        IDocumentRepository documentRepository,
        ICommentRepository commentRepository,
        ICurrentUserService currentUserService,
        ICommentService commentService,
        ITrustScoreService trustScoreService,
        IDateTimeProvider dateTimeProvider)
    {
        _reportRepository = reportRepository;
        _documentRepository = documentRepository;
        _commentRepository = commentRepository;
        _currentUserService = currentUserService;
        _commentService = commentService;
        _trustScoreService = trustScoreService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ReportDto> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create reports");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Kiểm tra trust score trước khi cho phép báo cáo
        var canCreateReport = await _trustScoreService.CanPerformActionAsync(
            userId,
            TrustScoreAction.CreateReport,
            cancellationToken);

        if (!canCreateReport)
        {
            var currentScore = await _trustScoreService.GetTrustScoreAsync(userId, cancellationToken);
            throw new UnauthorizedException(
                $"Bạn cần có trust score tối thiểu {TrustScoreConstants.MinimumScores[TrustScoreAction.CreateReport]} để báo cáo. " +
                $"Trust score hiện tại của bạn: {currentScore}");
        }

        // Validate: must have either DocumentId or CommentId, but not both
        if (!request.DocumentId.HasValue && !request.CommentId.HasValue)
            throw new BadRequestException("Either DocumentId or CommentId must be provided");

        if (request.DocumentId.HasValue && request.CommentId.HasValue)
            throw new BadRequestException("Cannot report both document and comment at the same time");

        // Validate document or comment exists
        if (request.DocumentId.HasValue)
        {
            var document = await _documentRepository.GetByIdAsync(request.DocumentId.Value, disableTracking: true, cancellationToken);
            if (document == null || document.IsDeleted)
                throw new NotFoundException($"Document with id {request.DocumentId.Value} not found");
        }

        if (request.CommentId.HasValue)
        {
            var comment = await _commentRepository.GetByIdAsync(request.CommentId.Value, disableTracking: true, cancellationToken);
            if (comment == null || comment.IsDeleted)
                throw new NotFoundException($"Comment with id {request.CommentId.Value} not found");
        }

        // Create report
        var report = new ReportDomain
        {
            Id = Guid.NewGuid(),
            DocumentId = request.DocumentId,
            CommentId = request.CommentId,
            Content = request.Content,
            ReviewStatus = ReviewStatus.PendingReview,
            CreatedById = userId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        await _reportRepository.AddAsync(report, cancellationToken);
        await _reportRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Get reporter information
        var authorInfo = await _commentService.GetCommentAuthorsAsync(new[] { userId }, cancellationToken);
        var reporter = authorInfo.TryGetValue(userId, out var reporterValue) ? reporterValue : null;

        return new ReportDto
        {
            Id = report.Id,
            DocumentId = report.DocumentId,
            CommentId = report.CommentId,
            Content = report.Content,
            ReporterName = reporter?.FullName ?? "Unknown",
            ReporterAvatarUrl = reporter?.AvatarUrl,
            CreatedById = report.CreatedById,
            ReviewStatus = report.ReviewStatus,
            CreatedAt = report.CreatedAt
        };
    }
}
