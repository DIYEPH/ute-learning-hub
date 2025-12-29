using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Tag.Queries.GetTypes;

namespace UteLearningHub.Application.Features.Type.Queries.GetTypes;

public record GetTypesQuery : GetTypesRequest, IRequest<PagedResponse<TypeDetailDto>>;
