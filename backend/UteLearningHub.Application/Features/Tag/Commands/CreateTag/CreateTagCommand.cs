using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Tag.Commands.CreateTag;

public record CreateTagCommand : CreateTagRequest, IRequest<TagDetailDto>;
