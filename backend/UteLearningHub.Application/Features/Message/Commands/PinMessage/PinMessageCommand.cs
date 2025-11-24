using MediatR;

namespace UteLearningHub.Application.Features.Message.Commands.PinMessage;

public record PinMessageCommand : PinMessageRequest, IRequest<Unit>;