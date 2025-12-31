using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Conversation.Commands.RespondToProposal;

public record RespondToProposalCommand : IRequest<RespondToProposalResponse>
{
    public Guid ConversationId { get; init; }
    public bool Accept { get; init; }
}

public record RespondToProposalResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
    public bool IsActivated { get; init; }
    public ConversationDto? Conversation { get; init; }
}

