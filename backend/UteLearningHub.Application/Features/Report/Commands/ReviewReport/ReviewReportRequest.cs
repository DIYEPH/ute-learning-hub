using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Report.Commands.ReviewReport;

public record ReviewReportRequest
{
    public Guid ReportId { get; init; }
    public ReviewStatus ReviewStatus { get; init; }
    public string? ReviewNote { get; init; }
}
