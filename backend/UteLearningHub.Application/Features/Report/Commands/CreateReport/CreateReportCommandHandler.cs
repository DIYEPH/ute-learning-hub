using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Comment;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using ReportEntity = UteLearningHub.Domain.Entities.Report;

namespace UteLearningHub.Application.Features.Report.Commands.CreateReport;

public class CreateReportCommandHandler : IRequestHandler<CreateReportCommand, ReportDto>
{
    private readonly IReportRepository _reportRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentService _commentService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUserService _userService;

    public CreateReportCommandHandler(
        IReportRepository reportRepository,
        IDocumentRepository documentRepository,
        ICommentRepository commentRepository,
        ICurrentUserService currentUserService,
        ICommentService commentService,
        IDateTimeProvider dateTimeProvider,
        IUserService userService)
    {
        _reportRepository = reportRepository;
        _documentRepository = documentRepository;
        _commentRepository = commentRepository;
        _currentUserService = currentUserService;
        _commentService = commentService;
        _dateTimeProvider = dateTimeProvider;
        _userService = userService;
    }

    public async Task<ReportDto> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create reports");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();
        var now = _dateTimeProvider.OffsetNow;

        var isAdmin = _currentUserService.IsInRole("Admin");
        var trustLevel = await _userService.GetTrustLevelAsync(userId, cancellationToken);

        var canCreateReport = isAdmin || (trustLevel.HasValue && trustLevel.Value >= TrustLever.Newbie);
        if (!canCreateReport)
            throw new UnauthorizedException("Only administrators or users with Newbie+ trust level can create reports");

        // Validate: must have either DocumentFileId or CommentId, but not both
        if (!request.DocumentFileId.HasValue && !request.CommentId.HasValue)
            throw new BadRequestException("Either DocumentFileId or CommentId must be provided");

        if (request.DocumentFileId.HasValue && request.CommentId.HasValue)
            throw new BadRequestException("Cannot report both document file and comment at the same time");

        // Validate document file or comment exists
        if (request.DocumentFileId.HasValue)
        {
            var documentFile = await _documentRepository.GetDocumentFileByIdAsync(
                request.DocumentFileId.Value, disableTracking: true, cancellationToken);
            if (documentFile == null || documentFile.IsDeleted)
                throw new NotFoundException($"DocumentFile with id {request.DocumentFileId.Value} not found");
        }

        if (request.CommentId.HasValue)
        {
            var comment = await _commentRepository.GetByIdAsync(
                request.CommentId.Value, disableTracking: true, cancellationToken);
            if (comment == null || comment.IsDeleted)
                throw new NotFoundException($"Comment with id {request.CommentId.Value} not found");
        }

        // Create report - always PendingReview, admin/moderator will review
        var report = new ReportEntity
        {
            Id = Guid.NewGuid(),
            DocumentFileId = request.DocumentFileId,
            CommentId = request.CommentId,
            Content = request.Content,
            Status = ContentStatus.PendingReview,
            CreatedById = userId,
            CreatedAt = now
        };

        await _reportRepository.AddAsync(report, cancellationToken);
        await _reportRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Get reporter information
        var authorInfo = await _commentService.GetCommentAuthorsAsync(new[] { userId }, cancellationToken);
        var reporter = authorInfo.TryGetValue(userId, out var reporterValue) ? reporterValue : null;

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
}


