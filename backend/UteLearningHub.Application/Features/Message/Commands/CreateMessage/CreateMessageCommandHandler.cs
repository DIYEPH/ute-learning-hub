using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Message;

namespace UteLearningHub.Application.Features.Message.Commands.CreateMessage;

public class CreateMessageCommandHandler(IMessageService messageService)
    : IRequestHandler<CreateMessageCommand, MessageDto>
{
    public async Task<MessageDto> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
        => await messageService.CreateAsync(request, cancellationToken);
}
