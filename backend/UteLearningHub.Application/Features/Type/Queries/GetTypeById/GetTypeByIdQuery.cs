using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Type.Queries.GetTypeById;

public record GetTypeByIdQuery : GetTypeByIdRequest, IRequest<TypeDetailDto>;
