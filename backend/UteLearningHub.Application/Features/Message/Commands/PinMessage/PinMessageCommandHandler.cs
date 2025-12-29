using MediatR;
using UteLearningHub.Application.Services.Message;

namespace UteLearningHub.Application.Features.Message.Commands.PinMessage;

public class PinMessageCommandHandler(IMessageService messageService)
    : IRequestHandler<PinMessageCommand>
{
    public async Task Handle(PinMessageCommand request, CancellationToken cancellationToken)
        => await messageService.PinAsync(request, cancellationToken);
}
