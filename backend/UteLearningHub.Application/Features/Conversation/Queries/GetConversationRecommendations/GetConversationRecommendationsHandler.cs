using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Cache;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetConversationRecommendations;

public class GetConversationRecommendationsHandler
    : IRequestHandler<GetConversationRecommendationsQuery, GetConversationRecommendationsResponse>
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IRecommendationService _recommendationService;
    private readonly IProfileVectorStore _profileVectorStore;
    private readonly IConversationVectorStore _conversationVectorStore;
    private readonly IConversationRepository _conversationRepository;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserDataRepository _userDataRepository;
    private readonly ICacheService _cache;
    private readonly ILogger<GetConversationRecommendationsHandler> _logger;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15); // Cache 15 phút

    public GetConversationRecommendationsHandler(
        IEmbeddingService embeddingService,
        IRecommendationService recommendationService,
        IProfileVectorStore profileVectorStore,
        IConversationVectorStore conversationVectorStore,
        IConversationRepository conversationRepository,
        IIdentityService identityService,
        ICurrentUserService currentUserService,
        IUserDataRepository userDataRepository,
        ICacheService cache,
        ILogger<GetConversationRecommendationsHandler> logger)
    {
        _embeddingService = embeddingService;
        _recommendationService = recommendationService;
        _profileVectorStore = profileVectorStore;
        _conversationVectorStore = conversationVectorStore;
        _conversationRepository = conversationRepository;
        _identityService = identityService;
        _currentUserService = currentUserService;
        _userDataRepository = userDataRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<GetConversationRecommendationsResponse> Handle(
        GetConversationRecommendationsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetConversationRecommendations API called");

        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to get recommendations");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();
        _logger.LogInformation("Getting recommendations for userId {UserId}", userId);

        // [DISABLED] Cache - uncomment to enable 15-minute cache
        // var cacheKey = $"recommendations:{userId}:{request.TopK}:{request.MinSimilarity}";
        // var cachedResponse = await _cache.GetAsync<GetConversationRecommendationsResponse>(cacheKey, cancellationToken);
        // if (cachedResponse != null)
        // {
        //     _logger.LogInformation("Returning CACHED recommendations for user {UserId}", userId);
        //     return cachedResponse;
        // }

        // Calculate user vector
        var userVector = await GetOrCalcUserVectorAsync(userId, cancellationToken);
        _logger.LogInformation("Got user vector: {Len} dims", userVector.Length);

        // Lấy tất cả conversations active và chưa join (bao gồm SubjectMajors để lấy FacultyId)
        var activeConversations = await _conversationRepository.GetQueryableSet()
            .Include(c => c.Subject)
                .ThenInclude(s => s!.SubjectMajors)
                    .ThenInclude(sm => sm.Major)
            .Include(c => c.ConversationTags)
                .ThenInclude(ct => ct.Tag)
            .Include(c => c.Members)
            .Include(c => c.ConversationJoinRequests)
            .AsNoTracking()
            .Where(c => !c.IsDeleted
                && c.ConversationStatus == ConversationStatus.Active
                && !c.Members.Any(m => m.UserId == userId && !m.IsDeleted))
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} active conversations user hasn't joined", activeConversations.Count);

        if (!activeConversations.Any())
        {
            _logger.LogInformation("No active conversations found, returning empty");
            return new GetConversationRecommendationsResponse
            {
                Recommendations = Array.Empty<ConversationRecommendationDto>(),
                TotalProcessed = 0,
                ProcessingTimeMs = 0
            };
        }

        // Calculate conversation vectors
        var convVectors = new List<ConversationVectorData>();
        foreach (var conv in activeConversations)
        {
            var vec = await GetOrCalcConvVectorAsync(conv, cancellationToken);
            convVectors.Add(new ConversationVectorData(conv.Id, vec));
        }

        // Gọi AI service để lấy recommendations
        var topK = request.TopK ?? 10;
        var minSimilarity = request.MinSimilarity ?? 0.3f;

        _logger.LogInformation("Calling AI: {Count} convs, topK={TopK}", convVectors.Count, topK);

        var recResponse = await _recommendationService.GetRecommendationsAsync(
            userVector, convVectors, topK, minSimilarity, cancellationToken);

        // Map recommendations với conversation details
        var conversationDict = activeConversations.ToDictionary(c => c.Id);
        var currentUserId = _currentUserService.UserId;

        var recs = recResponse.Recommendations.Select(rec =>
            {
                if (!conversationDict.TryGetValue(rec.ConversationId, out var conversation))
                    return null;

                var isMember = currentUserId.HasValue &&
                    conversation.Members.Any(m => m.UserId == currentUserId.Value && !m.IsDeleted);

                var hasPendingJoinRequest = currentUserId.HasValue &&
                    conversation.ConversationJoinRequests.Any(jr => 
                        jr.CreatedById == currentUserId.Value && 
                        jr.Status == ContentStatus.PendingReview &&
                        !jr.IsDeleted);

                return new ConversationRecommendationDto
                {
                    ConversationId = rec.ConversationId,
                    ConversationName = conversation.ConversationName,
                    Similarity = rec.Similarity,
                    Rank = rec.Rank,
                    Subject = conversation.Subject != null ? new SubjectDto
                    {
                        Id = conversation.Subject.Id,
                        SubjectName = conversation.Subject.SubjectName,
                        SubjectCode = conversation.Subject.SubjectCode
                    } : null,
                    Tags = conversation.ConversationTags
                        .Select(ct => new TagDto
                        {
                            Id = ct.Tag.Id,
                            TagName = ct.Tag.TagName
                        })
                        .ToList(),
                    AvatarUrl = conversation.AvatarUrl,
                    MemberCount = conversation.Members.Count(m => !m.IsDeleted),
                    IsCurrentUserMember = currentUserId.HasValue ? isMember : null,
                    HasPendingJoinRequest = currentUserId.HasValue ? hasPendingJoinRequest : null
                };
            })
            .Where(r => r != null)
            .Cast<ConversationRecommendationDto>()
            .ToList();

        var response = new GetConversationRecommendationsResponse
        {
            Recommendations = recs,
            TotalProcessed = recResponse.TotalProcessed,
            ProcessingTimeMs = recResponse.ProcessingTimeMs
        };

        // [DISABLED] Cache save - uncomment to enable 15-minute cache
        // await _cache.SetAsync(cacheKey, response, CacheExpiration, cancellationToken);

        return response;
    }

    private async Task<float[]> GetOrCalcUserVectorAsync(Guid userId, CancellationToken ct)
    {
        var dim = _embeddingService.Dim;

        var existing = await _profileVectorStore.Query()
            .Where(v => v.UserId == userId && v.IsActive)
            .OrderByDescending(v => v.CalculatedAt)
            .FirstOrDefaultAsync(ct);

        if (existing != null && !string.IsNullOrEmpty(existing.EmbeddingJson))
        {
            try
            {
                var vec = System.Text.Json.JsonSerializer.Deserialize<float[]>(existing.EmbeddingJson);
                if (vec != null && vec.Length == dim) return vec;
            }
            catch { /* recalculate */ }
        }

        var data = await _userDataRepository.GetUserBehaviorTextDataAsync(userId, ct);
        if (data == null) throw new NotFoundException($"User {userId} not found");

        var req = new UserVectorRequest
        {
            Subjects = data.SubjectScores.Select(x => x.Name).ToList(),
            SubjectWeights = data.SubjectScores.Select(x => (float)x.Score).ToList(),
            Tags = data.TagScores.Select(x => x.Name).ToList(),
            TagWeights = data.TagScores.Select(x => (float)x.Score).ToList()
        };

        var result = await _embeddingService.UserVectorAsync(req, ct);

        // Save async
        _ = Task.Run(async () =>
        {
            try
            {
                await _profileVectorStore.UpsertAsync(new Domain.Entities.ProfileVector
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    EmbeddingJson = System.Text.Json.JsonSerializer.Serialize(result),
                    CalculatedAt = DateTimeOffset.UtcNow,
                    IsActive = true
                }, ct);
            }
            catch { }
        }, ct);

        return result;
    }

    private async Task<float[]> GetOrCalcConvVectorAsync(Domain.Entities.Conversation conv, CancellationToken ct)
    {
        var dim = _embeddingService.Dim;

        var existing = await _conversationVectorStore.Query()
            .Where(v => v.ConversationId == conv.Id && v.IsActive)
            .OrderByDescending(v => v.CalculatedAt)
            .FirstOrDefaultAsync(ct);

        if (existing != null && !string.IsNullOrEmpty(existing.EmbeddingJson))
        {
            try
            {
                var vec = System.Text.Json.JsonSerializer.Deserialize<float[]>(existing.EmbeddingJson);
                if (vec != null && vec.Length == dim) return vec;
            }
            catch { /* recalculate */ }
        }

        var req = new ConvVectorRequest
        {
            Name = conv.ConversationName,
            Subject = conv.Subject?.SubjectName,
            Tags = conv.ConversationTags.Where(t => t.Tag != null).Select(t => t.Tag!.TagName).ToList()
        };

        var result = await _embeddingService.ConvVectorAsync(req, ct);

        // Save async
        _ = Task.Run(async () =>
        {
            try
            {
                await _conversationVectorStore.UpsertAsync(new Domain.Entities.ConversationVector
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conv.Id,
                    EmbeddingJson = System.Text.Json.JsonSerializer.Serialize(result),
                    CalculatedAt = DateTimeOffset.UtcNow,
                    IsActive = true
                }, ct);
            }
            catch { }
        }, ct);

        return result;
    }
}
