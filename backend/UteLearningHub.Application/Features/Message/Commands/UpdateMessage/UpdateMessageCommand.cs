using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Message.Commands.UpdateMessage;

public record UpdateMessageCommand : UpdateMessageRequest, IRequest<MessageDto>;