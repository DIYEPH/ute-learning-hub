using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Type.Queries.GetTypes;

public record GetTypesQuery : GetTypesRequest, IRequest<PagedResponse<TypeDto>>;
