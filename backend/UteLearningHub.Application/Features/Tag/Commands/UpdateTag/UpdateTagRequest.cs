namespace UteLearningHub.Application.Features.Tag.Commands.UpdateTag;

public record UpdateTagRequest
{
    public Guid Id { get; init; }
    public string TagName { get; init; } = default!;
}