using MediatR;

namespace UteLearningHub.Application.Features.Tag.Commands.DeleteTag;

public record DeleteTagCommand : IRequest
{
    public Guid Id { get; init; }
}
