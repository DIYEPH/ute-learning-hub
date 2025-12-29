using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Message.Commands.CreateMessage;
using UteLearningHub.Application.Features.Message.Commands.DeleteMessage;
using UteLearningHub.Application.Features.Message.Commands.MarkMessageAsRead;
using UteLearningHub.Application.Features.Message.Commands.PinMessage;
using UteLearningHub.Application.Features.Message.Commands.UpdateMessage;

namespace UteLearningHub.Application.Services.Message;

public interface IMessageService
{
    Task<MessageDto> CreateAsync(CreateMessageCommand request, CancellationToken ct = default);
    Task<MessageDto> UpdateAsync(UpdateMessageCommand request, CancellationToken ct = default);
    Task DeleteAsync(DeleteMessageCommand request, CancellationToken ct = default);
    Task PinAsync(PinMessageCommand request, CancellationToken ct = default);
    Task MarkAsReadAsync(MarkMessageAsReadCommand request, CancellationToken ct = default);
}
