using MediatR;
using UteLearningHub.Application.Services.Report;

namespace UteLearningHub.Application.Features.Report.Commands.ReviewReport;

public class ReviewReportCommandHandler(IReportService reportService) : IRequestHandler<ReviewReportCommand, Unit>
{
    public async Task<Unit> Handle(ReviewReportCommand request, CancellationToken ct)
    {
        await reportService.ReviewAsync(request, ct);
        return Unit.Value;
    }
}
