using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UteLearningHub.Application.Services.Cache;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.Recommendation;

public class VectorMaintenanceService : IVectorMaintenanceService
{
    private readonly IVectorCalculationService _vectorCalculationService;
    private readonly IProfileVectorStore _profileVectorStore;
    private readonly IConversationVectorStore _conversationVectorStore;
    private readonly IUserDataRepository _userDataRepository;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly ICacheService _cache;
    private readonly ILogger<VectorMaintenanceService> _logger;
    private const int VectorDimension = 100;

    public VectorMaintenanceService(
        IVectorCalculationService vectorCalculationService,
        IProfileVectorStore profileVectorStore,
        IConversationVectorStore conversationVectorStore,
        IUserDataRepository userDataRepository,
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        ICacheService cache,
        ILogger<VectorMaintenanceService> logger)
    {
        _vectorCalculationService = vectorCalculationService;
        _profileVectorStore = profileVectorStore;
        _conversationVectorStore = conversationVectorStore;
        _userDataRepository = userDataRepository;
        _dbContextFactory = dbContextFactory;
        _cache = cache;
        _logger = logger;
    }

    public async Task UpdateUserVectorAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use new behavior-based data
            var behaviorData = await _userDataRepository.GetUserBehaviorDataAsync(userId, cancellationToken);
            if (behaviorData == null)
            {
                _logger.LogWarning("User behavior data not found for userId {UserId}", userId);
                return;
            }

            // Use new behavior-based calculation
            var vector = _vectorCalculationService.CalculateUserVectorFromBehavior(
                behaviorData.FacultyScores,
                behaviorData.TypeScores,
                behaviorData.TagScores,
                VectorDimension);

            var profileVector = new UteLearningHub.Domain.Entities.ProfileVector
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                VectorType = ProfileVectorType.UserSubject,
                VectorDimension = VectorDimension,
                EmbeddingJson = System.Text.Json.JsonSerializer.Serialize(vector),
                CalculatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            };

            await _profileVectorStore.UpsertAsync(profileVector, cancellationToken);
            _logger.LogInformation("Updated user vector for userId {UserId} (behavior-based)", userId);

            // Invalidate cache
            await InvalidateUserRecommendationsCacheAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user vector for userId {UserId}", userId);
        }
    }

    public async Task UpdateConversationVectorAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            
            // Get conversation with Subject → SubjectMajors → Major (to get FacultyId)
            var conversation = await dbContext.Conversations
                .Include(c => c.Subject)
                    .ThenInclude(s => s!.SubjectMajors)
                        .ThenInclude(sm => sm.Major)
                .Include(c => c.ConversationTags)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted, cancellationToken);

            if (conversation == null)
            {
                _logger.LogWarning("Conversation not found for conversationId {ConversationId}", conversationId);
                return;
            }

            // Get FacultyIds from Subject → SubjectMajors → Major → FacultyId
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

            // Use new Faculty-based calculation
            var vector = _vectorCalculationService.CalculateConversationVector(
                facultyIds.Distinct().ToList(),
                tagIds,
                VectorDimension);

            var conversationVector = new UteLearningHub.Domain.Entities.ConversationVector
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SubjectId = conversation.SubjectId,
                VectorType = ProfileVectorType.ConversationTopic,
                VectorDimension = VectorDimension,
                EmbeddingJson = System.Text.Json.JsonSerializer.Serialize(vector),
                CalculatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            };

            await _conversationVectorStore.UpsertAsync(conversationVector, cancellationToken);
            _logger.LogInformation("Updated conversation vector for conversationId {ConversationId} (Faculty-based)", conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating conversation vector for conversationId {ConversationId}", conversationId);
        }
    }

    public async Task InvalidateUserRecommendationsCacheAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var pattern = $"recommendations:{userId}:*";
            await _cache.RemoveByPatternAsync(pattern, cancellationToken);
            _logger.LogDebug("Invalidated recommendations cache for userId {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating cache for userId {UserId}", userId);
        }
    }
}
