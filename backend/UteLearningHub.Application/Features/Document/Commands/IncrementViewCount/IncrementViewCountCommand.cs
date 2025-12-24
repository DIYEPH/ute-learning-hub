using MediatR;

namespace UteLearningHub.Application.Features.Document.Commands.IncrementViewCount;

public record IncrementViewCountCommand : IRequest<Unit>
{
    public Guid DocumentFileId { get; init; }
}
