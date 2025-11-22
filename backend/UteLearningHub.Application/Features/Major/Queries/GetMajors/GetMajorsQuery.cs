using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Major.Queries.GetMajors;

public record GetMajorsQuery : GetMajorsRequest, IRequest<PagedResponse<MajorDto>>;
