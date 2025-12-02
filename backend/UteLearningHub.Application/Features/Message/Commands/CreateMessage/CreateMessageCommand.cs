using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Message.Commands.CreateMessage;

public record CreateMessageCommand : CreateMessageRequest, IRequest<MessageDto>;