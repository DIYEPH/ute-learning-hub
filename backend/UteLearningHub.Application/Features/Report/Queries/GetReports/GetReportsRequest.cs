using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Report.Queries.GetReports;

public record GetReportsRequest : PagedRequest
{
    public Guid? DocumentId { get; init; }
    public Guid? CommentId { get; init; }
    public ReviewStatus? ReviewStatus { get; init; }
    public string? SearchTerm { get; init; }
}