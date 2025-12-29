using MediatR;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Conversation.Commands.ReviewConversationJoinRequest;

public record ReviewConversationJoinRequestCommand : IRequest
{
    public Guid JoinRequestId { get; init; }
    public ContentStatus Status { get; init; }
    public string? ReviewNote { get; init; }
}
