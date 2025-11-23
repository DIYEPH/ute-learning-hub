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
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        // Filters
        if (request.SubjectId.HasValue)
            query = query.Where(c => c.SubjectId == request.SubjectId.Value);

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
                c.Topic.ToLower().Contains(searchTerm));
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

        // Get creator information
        var creatorIds = conversations.Select(c => c.CreatedById).Distinct();
        var creatorInfo = new Dictionary<Guid, (string FullName, string? AvatarUrl)>();

        foreach (var creatorId in creatorIds)
        {
            var creator = await _identityService.FindByIdAsync(creatorId);
            if (creator != null)
            {
                creatorInfo[creatorId] = (creator.FullName, creator.AvatarUrl);
            }
        }

        var conversationDtos = conversations.Select(c => new ConversationDto
        {
            Id = c.Id,
            ConversationName = c.ConversationName,
            Topic = c.Topic,
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
            CreatorName = creatorInfo.TryGetValue(c.CreatedById, out var info)
                ? info.FullName
                : "Unknown",
            CreatorAvatarUrl = creatorInfo.TryGetValue(c.CreatedById, out var creator)
                ? creator.AvatarUrl
                : null,
            MemberCount = c.Members.Count(m => !m.IsDeleted),
            MessageCount = 0, // Will be calculated if needed
            LastMessageId = c.LastMessage,
            CreatedById = c.CreatedById,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
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
