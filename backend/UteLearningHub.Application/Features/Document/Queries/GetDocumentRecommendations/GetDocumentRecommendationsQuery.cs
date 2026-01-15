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
    public IReadOnlyList<DocumentDto> Recommendations { get; init; } = [];
    public int TotalProcessed { get; init; }
    public double ProcessingTimeMs { get; init; }
}
