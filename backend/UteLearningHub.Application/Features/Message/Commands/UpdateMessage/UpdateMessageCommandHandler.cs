using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Message;

namespace UteLearningHub.Application.Features.Message.Commands.UpdateMessage;

public class UpdateMessageCommandHandler(IMessageService messageService)
    : IRequestHandler<UpdateMessageCommand, MessageDto>
{
    public async Task<MessageDto> Handle(UpdateMessageCommand request, CancellationToken cancellationToken)
        => await messageService.UpdateAsync(request, cancellationToken);
}
