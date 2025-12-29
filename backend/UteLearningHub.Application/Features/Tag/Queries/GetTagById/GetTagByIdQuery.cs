using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Tag.Queries.GetTagById;

public record GetTagByIdQuery : IRequest<TagDetailDto>
{
    public Guid Id { get; init; }
}
