namespace UteLearningHub.Application.Features.Type.Commands.CreateType;

public record CreateTypeRequest
{
    public string TypeName { get; init; } = default!;
}