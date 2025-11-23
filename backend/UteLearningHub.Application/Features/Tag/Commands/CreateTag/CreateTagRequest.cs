namespace UteLearningHub.Application.Features.Tag.Commands.CreateTag;

public record CreateTagRequest
{
    public string TagName { get; init; } = default!;
}