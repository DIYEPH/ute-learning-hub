namespace UteLearningHub.Application.Features.Type.Commands.UpdateType;

public record UpdateTypeRequest
{
    public Guid Id { get; init; }
    public string TypeName { get; init; } = default!;
}