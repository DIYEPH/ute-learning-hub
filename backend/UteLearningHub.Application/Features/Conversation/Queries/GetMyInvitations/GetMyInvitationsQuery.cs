using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetMyInvitations;

public record GetMyInvitationsQuery : IRequest<PagedResponse<InvitationDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public bool PendingOnly { get; init; } = true;
}
