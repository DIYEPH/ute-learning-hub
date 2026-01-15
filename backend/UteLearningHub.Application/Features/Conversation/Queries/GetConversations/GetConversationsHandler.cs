using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetConversations;

public class GetConversationsHandler : IRequestHandler<GetConversationsQuery, PagedResponse<ConversationDto>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;

    public GetConversationsHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IIdentityService identityService,
        ICurrentUserService currentUserService)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _identityService = identityService;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResponse<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.IsAuthenticated ? _currentUserService.UserId : null;

        var query = _conversationRepository.GetQueryableSet()
            .Include(c => c.Subject)
            .Include(c => c.Members)
            .Include(c => c.ConversationJoinRequests)
            .Include(c => c.ConversationTags)
                .ThenInclude(ct => ct.Tag)
            .AsNoTracking();

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

        if (request.Visibility.HasValue)
            query = query.Where(c => c.Visibility == request.Visibility.Value);

        if (request.ConversationStatus.HasValue)
            query = query.Where(c => c.ConversationStatus == request.ConversationStatus.Value);
        else
            query = query.Where(c => c.ConversationStatus == Domain.Constaints.Enums.ConversationStatus.Active);

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

        var conversationIds = conversations.Select(c => c.Id).ToList();
        var unreadCounts = new Dictionary<Guid, int>();

        if (currentUserId.HasValue && conversationIds.Count > 0)
        {
            var memberInfos = conversations
                .SelectMany(c => c.Members)
                .Where(m => m.UserId == currentUserId.Value && !m.IsDeleted)
                .ToDictionary(m => m.ConversationId, m => m);

            // Query messages with IgnoreQueryFilters to include soft-deleted messages
            var messagesQuery = _messageRepository.GetQueryableSet()
                .IgnoreQueryFilters()
                .Where(m => conversationIds.Contains(m.ConversationId));

            // Get all relevant messages grouped by conversation
            var messagesByConversation = await messagesQuery
                .GroupBy(m => m.ConversationId)
                .Select(g => new
                {
                    ConversationId = g.Key,
                    Messages = g.Select(m => new { m.Id, m.CreatedAt, m.CreatedById }).ToList()
                })
                .ToListAsync(cancellationToken);

            foreach (var group in messagesByConversation)
            {
                if (!memberInfos.TryGetValue(group.ConversationId, out var member))
                    continue;

                int count;
                if (member.LastReadMessageId == null)
                {
                    // Never read - all messages are unread (except own)
                    count = group.Messages.Count(m => m.CreatedById != currentUserId.Value);
                }
                else
                {
                    var lastReadMessage = group.Messages.FirstOrDefault(m => m.Id == member.LastReadMessageId);
                    if (lastReadMessage != null)
                    {
                        count = group.Messages.Count(m =>
                            m.CreatedAt > lastReadMessage.CreatedAt &&
                            m.CreatedById != currentUserId.Value);
                    }
                    else
                    {
                        // LastReadMessage not found, count all as unread
                        count = group.Messages.Count(m => m.CreatedById != currentUserId.Value);
                    }
                }
                unreadCounts[group.ConversationId] = count;
            }
        }

        var conversationDtos = conversations.Select(c =>
        {
            var isMember = currentUserId.HasValue && c.Members.Any(m => m.UserId == currentUserId.Value && !m.IsDeleted);
            var hasPendingRequest = currentUserId.HasValue && c.ConversationJoinRequests.Any(r =>
                r.CreatedById == currentUserId.Value &&
                r.Status == Domain.Constaints.Enums.ContentStatus.PendingReview &&
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
                Visibility = c.Visibility,
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
                MemberCount = c.Members.Count(m => !m.IsDeleted && m.InviteStatus == Domain.Constaints.Enums.MemberInviteStatus.Joined),
                UnreadCount = unreadCounts.GetValueOrDefault(c.Id, 0),
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
