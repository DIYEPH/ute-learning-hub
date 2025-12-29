using MediatR;
using UteLearningHub.Application.Services.Message;

namespace UteLearningHub.Application.Features.Message.Commands.DeleteMessage;

public class DeleteMessageCommandHandler(IMessageService messageService)
    : IRequestHandler<DeleteMessageCommand>
{
    public async Task Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
        => await messageService.DeleteAsync(request, cancellationToken);
}
