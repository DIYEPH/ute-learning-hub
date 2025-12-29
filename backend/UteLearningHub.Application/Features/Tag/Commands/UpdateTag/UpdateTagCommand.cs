using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Tag.Commands.UpdateTag;

public record UpdateTagCommand : UpdateTagCommandRequest, IRequest<TagDetailDto>
{
    public Guid Id { get; init; }
}
