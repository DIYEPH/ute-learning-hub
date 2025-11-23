using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Type.Commands.CreateType;

public record CreateTypeCommand : CreateTypeRequest, IRequest<TypeDetailDto>;
