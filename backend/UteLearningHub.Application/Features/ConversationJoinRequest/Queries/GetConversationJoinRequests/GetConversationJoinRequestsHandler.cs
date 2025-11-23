using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.ConversationJoinRequest.Queries.GetConversationJoinRequests;

public class GetConversationJoinRequestsHandler : IRequestHandler<GetConversationJoinRequestsQuery, PagedResponse<ConversationJoinRequestDto>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;

    public GetConversationJoinRequestsHandler(
        IConversationRepository conversationRepository,
        IIdentityService identityService,
        ICurrentUserService currentUserService)
    {
        _conversationRepository = conversationRepository;
        _identityService = identityService;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResponse<ConversationJoinRequestDto>> Handle(GetConversationJoinRequestsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to view join requests");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Check permission: Admin or Owner of the conversation
        var isAdmin = _currentUserService.IsInRole("Admin");

        // Non-admin users must specify a ConversationId
        if (!isAdmin && !request.ConversationId.HasValue)
            throw new BadRequestException("ConversationId is required to view join requests");

        // If filtering by conversation, check if user is owner
        bool isOwner = false;
        if (request.ConversationId.HasValue)
        {
            // Verify conversation exists and is Private
            var conversation = await _conversationRepository.GetByIdAsync(
                request.ConversationId.Value, 
                disableTracking: true, 
                cancellationToken);

            if (conversation == null || conversation.IsDeleted)
                throw new NotFoundException($"Conversation with id {request.ConversationId.Value} not found");

            if (conversation.ConversationType != ConversitionType.Private)
                throw new BadRequestException("Join requests are only available for private conversations");

            // Check if user is owner
            if (!isAdmin)
            {
                isOwner = await _conversationRepository.GetQueryableSet()
                    .Where(c => c.Id == request.ConversationId.Value)
                    .SelectMany(c => c.Members)
                    .AnyAsync(m => m.UserId == userId 
                                && m.ConversationMemberRoleType == ConversationMemberRoleType.Owner 
                                && !m.IsDeleted, cancellationToken);

                if (!isOwner)
                    throw new UnauthorizedException("Only conversation owners can view join requests for this conversation");
            }
        }

        var query = _conversationRepository.GetJoinRequestsQueryable()
            .Include(r => r.Conversation)
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.Conversation.ConversationType == ConversitionType.Private); // Chỉ lấy Private conversations

        // Filters
        if (request.ConversationId.HasValue)
            query = query.Where(r => r.ConversationId == request.ConversationId.Value);

        if (request.CreatedById.HasValue)
            query = query.Where(r => r.CreatedById == request.CreatedById.Value);

        if (request.ReviewStatus.HasValue)
            query = query.Where(r => r.ReviewStatus == request.ReviewStatus.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(r => r.Content.ToLower().Contains(searchTerm) 
                                  || r.Conversation.ConversationName.ToLower().Contains(searchTerm));
        }

        // Order by newest first
        query = query.OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var joinRequests = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        // Get requester information
        var requesterIds = joinRequests.Select(r => r.CreatedById).Distinct();
        var requesterInfo = new Dictionary<Guid, (string FullName, string? AvatarUrl)>();
        
        foreach (var requesterId in requesterIds)
        {
            var requester = await _identityService.FindByIdAsync(requesterId);
            if (requester != null)
            {
                requesterInfo[requesterId] = (requester.FullName, requester.AvatarUrl);
            }
        }

        var joinRequestDtos = joinRequests.Select(r => new ConversationJoinRequestDto
        {
            Id = r.Id,
            ConversationId = r.ConversationId,
            ConversationName = r.Conversation.ConversationName,
            Content = r.Content,
            RequesterName = requesterInfo.TryGetValue(r.CreatedById, out var info) 
                ? info.FullName 
                : "Unknown",
            RequesterAvatarUrl = requesterInfo.TryGetValue(r.CreatedById, out var requester) 
                ? requester.AvatarUrl 
                : null,
            CreatedById = r.CreatedById,
            ReviewStatus = r.ReviewStatus,
            ReviewNote = r.ReviewNote,
            ReviewedAt = r.ReviewedAt,
            CreatedAt = r.CreatedAt
        }).ToList();

        return new PagedResponse<ConversationJoinRequestDto>
        {
            Items = joinRequestDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
