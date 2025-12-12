using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Report.Queries.GetReports;

public record GetReportsRequest : PagedRequest
{
    public Guid? DocumentFileId { get; init; }
    public Guid? CommentId { get; init; }
    public ContentStatus? Status { get; init; }
    public string? SearchTerm { get; init; }
}