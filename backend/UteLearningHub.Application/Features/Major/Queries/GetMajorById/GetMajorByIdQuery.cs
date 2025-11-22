using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Major.Queries.GetMajorById;

public record GetMajorByIdQuery : GetMajorByIdRequest, IRequest<MajorDetailDto>;