using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Major.Commands.CreateMajor;

public record CreateMajorCommand : CreateMajorRequest, IRequest<MajorDetailDto>;