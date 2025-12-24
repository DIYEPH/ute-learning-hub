using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetMyInvitations;

public class GetMyInvitationsHandler : IRequestHandler<GetMyInvitationsQuery, PagedResponse<InvitationDto>>
{
    private readonly IConversationRepository _convRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;

    public GetMyInvitationsHandler(
        IConversationRepository convRepo,
        ICurrentUserService currentUser,
        IIdentityService identityService)
    {
        _convRepo = convRepo;
        _currentUser = currentUser;
        _identityService = identityService;
    }

    public async Task<PagedResponse<InvitationDto>> Handle(GetMyInvitationsQuery request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedException("Must be authenticated");

        var userId = _currentUser.UserId ?? throw new UnauthorizedException();

        var query = _convRepo.GetInvitationsQueryable()
            .Include(i => i.Conversation)
                .ThenInclude(c => c.Members)
            .Where(i => i.InvitedUserId == userId && !i.IsDeleted);

        if (request.PendingOnly)
        {
            query = query.Where(i => i.Status == ContentStatus.PendingReview);
        }

        var totalCount = await query.CountAsync(ct);

        var invitations = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        // Get inviter info
        var inviterIds = invitations.Select(i => i.CreatedById).Distinct().ToList();
        var inviters = new Dictionary<Guid, (string Name, string? Avatar)>();
        foreach (var inviterId in inviterIds)
        {
            var user = await _identityService.FindByIdAsync(inviterId);
            if (user != null)
            {
                inviters[inviterId] = (user.FullName, user.AvatarUrl);
            }
        }

        var items = invitations.Select(i => new InvitationDto
        {
            Id = i.Id,
            ConversationId = i.ConversationId,
            ConversationName = i.Conversation.ConversationName,
            ConversationAvatarUrl = i.Conversation.AvatarUrl,
            MemberCount = i.Conversation.Members.Count(m => !m.IsDeleted),
            InvitedById = i.CreatedById,
            InvitedByName = inviters.TryGetValue(i.CreatedById, out var inviter) ? inviter.Name : "Unknown",
            InvitedByAvatarUrl = inviters.TryGetValue(i.CreatedById, out var inv) ? inv.Avatar : null,
            Message = i.Message,
            Status = i.Status,
            CreatedAt = i.CreatedAt
        }).ToList();

        return new PagedResponse<InvitationDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
