using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Application.Services.Cache;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetConversationRecommendations;

public class GetConversationRecommendationsHandler 
    : IRequestHandler<GetConversationRecommendationsQuery, GetConversationRecommendationsResponse>
{
    private readonly IVectorCalculationService _vectorCalculationService;
    private readonly IRecommendationService _recommendationService;
    private readonly IProfileVectorStore _profileVectorStore;
    private readonly IConversationVectorStore _conversationVectorStore;
    private readonly IConversationRepository _conversationRepository;
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserDataRepository _userDataRepository;
    private readonly ICacheService _cache;
    private readonly int _vectorDimension;
    private readonly ILogger<GetConversationRecommendationsHandler> _logger;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15); // Cache 15 phút

    public GetConversationRecommendationsHandler(
        IVectorCalculationService vectorCalculationService,
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
        _vectorCalculationService = vectorCalculationService;
        _recommendationService = recommendationService;
        _profileVectorStore = profileVectorStore;
        _conversationVectorStore = conversationVectorStore;
        _conversationRepository = conversationRepository;
        _identityService = identityService;
        _currentUserService = currentUserService;
        _userDataRepository = userDataRepository;
        _cache = cache;
        _vectorDimension = 100; // Default vector dimension
        _logger = logger;
    }

    public async Task<GetConversationRecommendationsResponse> Handle(
        GetConversationRecommendationsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to get recommendations");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Kiểm tra cache
        var cacheKey = $"recommendations:{userId}:{request.TopK}:{request.MinSimilarity}";
        var cachedResponse = await _cache.GetAsync<GetConversationRecommendationsResponse>(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            _logger.LogDebug("Returning cached recommendations for user {UserId}", userId);
            return cachedResponse;
        }

        // Lấy hoặc tính user vector
        var userVector = await GetOrCalculateUserVectorAsync(userId, cancellationToken);

        // Lấy tất cả conversations active và chưa join (bao gồm SubjectMajors để lấy FacultyId)
        var activeConversations = await _conversationRepository.GetQueryableSet()
            .Include(c => c.Subject)
                .ThenInclude(s => s!.SubjectMajors)
                    .ThenInclude(sm => sm.Major)
            .Include(c => c.ConversationTags)
                .ThenInclude(ct => ct.Tag)
            .Include(c => c.Members)
            .AsNoTracking()
            .Where(c => !c.IsDeleted 
                && c.ConversationStatus == ConversationStatus.Active
                && !c.Members.Any(m => m.UserId == userId && !m.IsDeleted))
            .ToListAsync(cancellationToken);

        if (!activeConversations.Any())
        {
            return new GetConversationRecommendationsResponse
            {
                Recommendations = Array.Empty<ConversationRecommendationDto>(),
                TotalProcessed = 0,
                ProcessingTimeMs = 0
            };
        }

        // Lấy hoặc tính conversation vectors
        var conversationVectors = new List<ConversationVectorData>();
        foreach (var conversation in activeConversations)
        {
            // Lấy FacultyIds từ Subject → SubjectMajors → Major
            var facultyIds = new List<Guid>();
            if (conversation.Subject != null)
            {
                foreach (var sm in conversation.Subject.SubjectMajors)
                {
                    if (sm.Major != null && !sm.Major.IsDeleted)
                    {
                        facultyIds.Add(sm.Major.FacultyId);
                    }
                }
            }

            var tagIds = conversation.ConversationTags
                .Select(ct => ct.TagId)
                .ToList();

            var convVector = await GetOrCalculateConversationVectorAsync(
                conversation.Id,
                conversation.SubjectId,
                facultyIds.Distinct().ToList(),
                tagIds,
                cancellationToken);

            conversationVectors.Add(new ConversationVectorData(conversation.Id, convVector));
        }

        // Gọi AI service để lấy recommendations
        var topK = request.TopK ?? 10;
        var minSimilarity = request.MinSimilarity ?? 0.3f;

        var recommendationResponse = await _recommendationService.GetRecommendationsAsync(
            userVector,
            conversationVectors,
            topK,
            minSimilarity,
            cancellationToken);

        // Map recommendations với conversation details
        var conversationDict = activeConversations.ToDictionary(c => c.Id);
        var currentUserId = _currentUserService.UserId;

        var recommendations = recommendationResponse.Recommendations
            .Select(rec =>
            {
                if (!conversationDict.TryGetValue(rec.ConversationId, out var conversation))
                    return null;

                var isMember = currentUserId.HasValue && 
                    conversation.Members.Any(m => m.UserId == currentUserId.Value && !m.IsDeleted);

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
                    IsCurrentUserMember = currentUserId.HasValue ? isMember : null
                };
            })
            .Where(r => r != null)
            .Cast<ConversationRecommendationDto>()
            .ToList();

        var response = new GetConversationRecommendationsResponse
        {
            Recommendations = recommendations,
            TotalProcessed = recommendationResponse.TotalProcessed,
            ProcessingTimeMs = recommendationResponse.ProcessingTimeMs
        };

        // Lưu vào cache
        await _cache.SetAsync(cacheKey, response, CacheExpiration, cancellationToken);
        _logger.LogDebug("Cached recommendations for user {UserId}", userId);

        return response;
    }

    private async Task<float[]> GetOrCalculateUserVectorAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Tìm vector đã lưu trong DB
        var existingVector = await _profileVectorStore.Query()
            .Where(v => v.UserId == userId 
                && v.VectorType == ProfileVectorType.UserSubject 
                && v.IsActive)
            .OrderByDescending(v => v.CalculatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingVector != null && !string.IsNullOrEmpty(existingVector.EmbeddingJson))
        {
            try
            {
                var savedVector = System.Text.Json.JsonSerializer.Deserialize<float[]>(existingVector.EmbeddingJson);
                if (savedVector != null && savedVector.Length == _vectorDimension)
                {
                    return savedVector;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize user vector from DB, will recalculate");
            }
        }

        // Tính vector mới từ behavior data
        var behaviorData = await _userDataRepository.GetUserBehaviorDataAsync(userId, cancellationToken);
        if (behaviorData == null)
            throw new NotFoundException($"User with id {userId} not found");

        var calculatedVector = _vectorCalculationService.CalculateUserVectorFromBehavior(
            behaviorData.FacultyScores,
            behaviorData.TypeScores,
            behaviorData.TagScores,
            _vectorDimension);

        // Lưu vào DB (async, không cần chờ)
        _ = Task.Run(async () =>
        {
            try
            {
                var profileVector = new UteLearningHub.Domain.Entities.ProfileVector
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    VectorType = ProfileVectorType.UserSubject,
                    VectorDimension = _vectorDimension,
                    EmbeddingJson = System.Text.Json.JsonSerializer.Serialize(calculatedVector),
                    CalculatedAt = DateTimeOffset.UtcNow,
                    IsActive = true
                };

                await _profileVectorStore.UpsertAsync(profileVector, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save user vector to DB");
            }
        }, cancellationToken);

        return calculatedVector;
    }

    private async Task<float[]> GetOrCalculateConversationVectorAsync(
        Guid conversationId,
        Guid? subjectId,
        IReadOnlyList<Guid> facultyIds,
        IReadOnlyList<Guid> tagIds,
        CancellationToken cancellationToken)
    {
        // Tìm vector đã lưu trong DB
        var existingVector = await _conversationVectorStore.Query()
            .Where(v => v.ConversationId == conversationId 
                && v.VectorType == ProfileVectorType.ConversationTopic 
                && v.IsActive)
            .OrderByDescending(v => v.CalculatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingVector != null && !string.IsNullOrEmpty(existingVector.EmbeddingJson))
        {
            try
            {
                var savedVector = System.Text.Json.JsonSerializer.Deserialize<float[]>(existingVector.EmbeddingJson);
                if (savedVector != null && savedVector.Length == _vectorDimension)
                {
                    return savedVector;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize conversation vector from DB, will recalculate");
            }
        }

        // Tính vector mới với Faculty-based approach
        var calculatedVector = _vectorCalculationService.CalculateConversationVector(
            facultyIds,
            tagIds,
            _vectorDimension);

        // Lưu vào DB (async, không cần chờ)
        _ = Task.Run(async () =>
        {
            try
            {
                var conversationVector = new UteLearningHub.Domain.Entities.ConversationVector
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    SubjectId = subjectId,
                    VectorType = ProfileVectorType.ConversationTopic,
                    VectorDimension = _vectorDimension,
                    EmbeddingJson = System.Text.Json.JsonSerializer.Serialize(calculatedVector),
                    CalculatedAt = DateTimeOffset.UtcNow,
                    IsActive = true
                };

                await _conversationVectorStore.UpsertAsync(conversationVector, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save conversation vector to DB");
            }
        }, cancellationToken);

        return calculatedVector;
    }
}

