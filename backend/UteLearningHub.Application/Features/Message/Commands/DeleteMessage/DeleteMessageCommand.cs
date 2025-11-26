using MediatR;

namespace UteLearningHub.Application.Features.Message.Commands.DeleteMessage;

public record DeleteMessageCommand : DeleteMessageRequest, IRequest<Unit>;