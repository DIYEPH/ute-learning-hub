
namespace UteLearningHub.Application.Common.Dtos;

public record TagDetailDto
{
    public Guid Id { get; init; }
    public string TagName { get; init; } = default!;
    public int DocumentCount { get; init; }
}


