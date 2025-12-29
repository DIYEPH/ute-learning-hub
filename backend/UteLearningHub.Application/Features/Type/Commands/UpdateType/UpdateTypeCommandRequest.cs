namespace UteLearningHub.Application.Features.Type.Commands.UpdateType;

public record UpdateTypeCommandRequest
{
    public string TypeName { get; init; } = default!;
}
