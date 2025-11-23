using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Type.Commands.UpdateType;

public record UpdateTypeCommand : UpdateTypeRequest, IRequest<TypeDetailDto>;
