using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Tag.Commands.CreateTag;

public record CreateTagCommand : IRequest<TagDetailDto>
{
    public string TagName { get; init; } = default!;
}
