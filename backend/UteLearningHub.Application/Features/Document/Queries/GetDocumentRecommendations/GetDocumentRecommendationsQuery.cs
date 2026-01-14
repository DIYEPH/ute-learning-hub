using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Document.Queries.GetDocumentRecommendations;

public record GetDocumentRecommendationsQuery : IRequest<GetDocumentRecommendationsResponse>
{
    public int? TopK { get; init; }
    public float? MinSimilarity { get; init; }
}

public record GetDocumentRecommendationsResponse
{
    public IReadOnlyList<DocumentRecommendationDto> Recommendations { get; init; } = [];
    public int TotalProcessed { get; init; }
    public double ProcessingTimeMs { get; init; }
}

public record DocumentRecommendationDto
{
    public Guid DocumentId { get; init; }
    public string DocumentName { get; init; } = default!;
    public string? Description { get; init; }
    public float Similarity { get; init; }
    public int Rank { get; init; }
    public SubjectDto? Subject { get; init; }
    public IList<TagDto> Tags { get; init; } = [];
    public string? CoverUrl { get; init; }
    public int FileCount { get; init; }
    public int UsefulCount { get; init; }
    public AuthorDto? Author { get; init; }
}
