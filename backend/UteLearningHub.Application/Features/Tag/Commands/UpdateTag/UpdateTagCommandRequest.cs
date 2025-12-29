namespace UteLearningHub.Application.Features.Tag.Commands.UpdateTag;

public record UpdateTagCommandRequest
{
    public string TagName { get; init; } = default!;
}
