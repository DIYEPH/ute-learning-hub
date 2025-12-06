using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetConversations;

public class GetConversationsHandler : IRequestHandler<GetConversationsQuery, PagedResponse<ConversationDto>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;

    public GetConversationsHandler(
        IConversationRepository conversationRepository,
        IIdentityService identityService,
        ICurrentUserService currentUserService)
    {
        _conversationRepository = conversationRepository;
        _identityService = identityService;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResponse<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var query = _conversationRepository.GetQueryableSet()
            .Include(c => c.Subject)
            .Include(c => c.Members)
            .Include(c => c.ConversationJoinRequests)
            .Include(c => c.ConversationTags)
                .ThenInclude(ct => ct.Tag)
            .AsNoTracking();

        // Filter by IsDeleted status (default: only active items)
        if (request.IsDeleted.HasValue)
            query = query.Where(c => c.IsDeleted == request.IsDeleted.Value);
        else
            query = query.Where(c => !c.IsDeleted);

        // Filters
        if (request.SubjectId.HasValue)
            query = query.Where(c => c.SubjectId == request.SubjectId.Value);

        if (request.TagId.HasValue)
            query = query.Where(c => c.ConversationTags.Any(ct => ct.TagId == request.TagId.Value));

        if (request.ConversationType.HasValue)
            query = query.Where(c => c.ConversationType == request.ConversationType.Value);

        if (request.ConversationStatus.HasValue)
            query = query.Where(c => c.ConversationStatus == request.ConversationStatus.Value);

        if (request.CreatedById.HasValue)
            query = query.Where(c => c.CreatedById == request.CreatedById.Value);

        if (request.MemberId.HasValue)
            query = query.Where(c => c.Members.Any(m => m.UserId == request.MemberId.Value && !m.IsDeleted));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(c =>
                c.ConversationName.ToLower().Contains(searchTerm) ||
                c.ConversationTags.Any(ct => ct.Tag.TagName.ToLower().Contains(searchTerm)));
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(c => c.ConversationName)
                : query.OrderBy(c => c.ConversationName),
            "createdat" or "date" => request.SortDescending
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderByDescending(c => c.CreatedAt) // Default: newest first
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var conversations = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        // Get current user ID if authenticated
        var currentUserId = _currentUserService.IsAuthenticated ? _currentUserService.UserId : null;

        var conversationDtos = conversations.Select(c =>
        {
            var isMember = currentUserId.HasValue && c.Members.Any(m => m.UserId == currentUserId.Value && !m.IsDeleted);
            var hasPendingRequest = currentUserId.HasValue && c.ConversationJoinRequests.Any(r =>
                r.CreatedById == currentUserId.Value &&
                r.ReviewStatus == Domain.Constaints.Enums.ReviewStatus.PendingReview &&
                !r.IsDeleted);

            return new ConversationDto
            {
                Id = c.Id,
                ConversationName = c.ConversationName,
                Tags = c.ConversationTags
                    .Select(ct => new TagDto
                    {
                        Id = ct.Tag.Id,
                        TagName = ct.Tag.TagName
                    })
                    .ToList(),
                ConversationType = c.ConversationType,
                ConversationStatus = c.ConversationStatus,
                IsSuggestedByAI = c.IsSuggestedByAI,
                IsAllowMemberPin = c.IsAllowMemberPin,
                Subject = c.Subject != null ? new SubjectDto
                {
                    Id = c.Subject.Id,
                    SubjectName = c.Subject.SubjectName,
                    SubjectCode = c.Subject.SubjectCode
                } : null,
                AvatarUrl = c.AvatarUrl,
                MemberCount = c.Members.Count(m => !m.IsDeleted),
                MessageCount = 0, 
                LastMessageId = c.LastMessage,
                CreatedById = c.CreatedById,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                IsCurrentUserMember = currentUserId.HasValue ? isMember : null,
                HasPendingJoinRequest = currentUserId.HasValue ? hasPendingRequest : null
            };
        }).ToList();

        return new PagedResponse<ConversationDto>
        {
            Items = conversationDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
