using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Message.Commands.CreateMessage;

public record CreateMessageCommand : IRequest<MessageDto>
{
    public Guid ConversationId { get; init; }
    public Guid? ParentId { get; init; }
    public string Content { get; init; } = default!;
    public IList<Guid>? FileIds { get; init; }
}