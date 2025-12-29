using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Report.Commands.CreateReport;
using UteLearningHub.Application.Features.Report.Commands.ReviewReport;

namespace UteLearningHub.Application.Services.Report;

public interface IReportService
{
    Task<ReportDto> CreateAsync(CreateReportCommand command, CancellationToken ct = default);
    Task<ReportDto> ReviewAsync(ReviewReportCommand command, CancellationToken ct = default);
}
