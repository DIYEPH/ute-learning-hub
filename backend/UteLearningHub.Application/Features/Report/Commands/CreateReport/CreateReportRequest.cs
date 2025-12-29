using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Report.Commands.CreateReport;

public record CreateReportRequest
{
    public Guid? DocumentFileId { get; init; }
    public Guid? CommentId { get; init; }
    public ReportReason Reason { get; init; } = ReportReason.Other;
    public string? Content { get; init; } = default!;
}
