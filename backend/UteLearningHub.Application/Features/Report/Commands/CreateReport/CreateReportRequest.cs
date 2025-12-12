namespace UteLearningHub.Application.Features.Report.Commands.CreateReport;

public record CreateReportRequest
{
    public Guid? DocumentFileId { get; init; }
    public Guid? CommentId { get; init; }
    public string Content { get; init; } = default!;
}
