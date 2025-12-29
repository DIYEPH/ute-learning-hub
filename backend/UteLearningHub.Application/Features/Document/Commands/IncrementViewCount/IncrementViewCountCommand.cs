using MediatR;

namespace UteLearningHub.Application.Features.Document.Commands.IncrementViewCount;

public record IncrementViewCountCommand : IRequest
{
    public Guid DocumentFileId { get; init; }
}
