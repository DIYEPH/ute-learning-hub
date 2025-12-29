using MediatR;
using UteLearningHub.Application.Services.Message;

namespace UteLearningHub.Application.Features.Message.Commands.MarkMessageAsRead;

public class MarkMessageAsReadCommandHandler(IMessageService messageService)
    : IRequestHandler<MarkMessageAsReadCommand>
{
    public async Task Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken)
        => await messageService.MarkAsReadAsync(request, cancellationToken);
}
