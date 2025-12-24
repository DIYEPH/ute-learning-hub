using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Conversation.Commands.GetOrCreateDM;

public record GetOrCreateDMCommand : IRequest<GetOrCreateDMResponse>
{
    public Guid TargetUserId { get; init; }
    public string? FirstMessage { get; init; }  // Required for new DM, optional for existing
}

public record GetOrCreateDMResponse
{
    public ConversationDetailDto Conversation { get; init; } = default!;
    public MessageDto? FirstMessageSent { get; init; }  // Null nếu DM đã tồn tại
    public bool IsNewConversation { get; init; }
}

