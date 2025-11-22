using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Report.Commands.ReviewReport;

public class ReviewReportCommandHandler : IRequestHandler<ReviewReportCommand, Unit>
{
    private readonly IReportRepository _reportRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReviewReportCommandHandler(
        IReportRepository reportRepository,
        ICurrentUserService currentUserService,
        IUserService userService,
        IDateTimeProvider dateTimeProvider)
    {
        _reportRepository = reportRepository;
        _currentUserService = currentUserService;
        _userService = userService;
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

        var canReview = isAdmin || 
                       (trustLevel.HasValue && trustLevel.Value >= TrustLever.Moderator);

        if (!canReview)
            throw new UnauthorizedException("Only administrators or moderators can review reports");

        var report = await _reportRepository.GetByIdAsync(request.ReportId, disableTracking: false, cancellationToken);

        if (report == null || report.IsDeleted)
            throw new NotFoundException($"Report with id {request.ReportId} not found");

        // Update review information
        report.ReviewStatus = request.ReviewStatus;
        report.ReviewedById = userId;
        report.ReviewedAt = _dateTimeProvider.OffsetNow;
        report.ReviewNote = request.ReviewNote;

        await _reportRepository.UpdateAsync(report, cancellationToken);
        await _reportRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}