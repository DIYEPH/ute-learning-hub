using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Type.Queries.GetTypeById;

public record GetTypeByIdQuery : IRequest<TypeDetailDto>
{
    public Guid Id { get; init; }
}
