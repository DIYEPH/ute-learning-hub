using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Major.Commands.UpdateMajor;

public record UpdateMajorCommand : UpdateMajorRequest, IRequest<MajorDetailDto>;
