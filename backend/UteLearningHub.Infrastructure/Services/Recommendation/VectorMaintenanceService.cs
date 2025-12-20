using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UteLearningHub.Application.Services.Cache;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.Recommendation;

public class VectorMaintenanceService : IVectorMaintenanceService
{
    private readonly IEmbeddingService _embed;
    private readonly IProfileVectorStore _userStore;
    private readonly IConversationVectorStore _convStore;
    private readonly IUserDataRepository _userData;
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ICacheService _cache;
    private readonly ILogger<VectorMaintenanceService> _log;

    public VectorMaintenanceService(
        IEmbeddingService embed,
        IProfileVectorStore userStore,
        IConversationVectorStore convStore,
        IUserDataRepository userData,
        IDbContextFactory<ApplicationDbContext> dbFactory,
        ICacheService cache,
        ILogger<VectorMaintenanceService> log)
    {
        _embed = embed;
        _userStore = userStore;
        _convStore = convStore;
        _userData = userData;
        _dbFactory = dbFactory;
        _cache = cache;
        _log = log;
    }

    public async Task UpdateUserVectorAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var data = await _userData.GetUserBehaviorTextDataAsync(userId, ct);
            if (data == null) return;

            var req = new UserVectorRequest
            {
                Subjects = data.SubjectScores.Select(x => x.Name).ToList(),
                SubjectWeights = data.SubjectScores.Select(x => (float)x.Score).ToList(),
                Tags = data.TagScores.Select(x => x.Name).ToList(),
                TagWeights = data.TagScores.Select(x => (float)x.Score).ToList()
            };

            var vec = await _embed.UserVectorAsync(req, ct);

            await _userStore.UpsertAsync(new Domain.Entities.ProfileVector
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EmbeddingJson = System.Text.Json.JsonSerializer.Serialize(vec),
                CalculatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            }, ct);

            _log.LogInformation("Updated user vector: {UserId}", userId);
            await InvalidateUserRecommendationsCacheAsync(userId, ct);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to update user vector: {UserId}", userId);
        }
    }

    public async Task UpdateConversationVectorAsync(Guid convId, CancellationToken ct = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);

            var conv = await db.Conversations
                .Include(c => c.Subject)
                .Include(c => c.ConversationTags).ThenInclude(ct => ct.Tag)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == convId && !c.IsDeleted, ct);

            if (conv == null) return;

            var req = new ConvVectorRequest
            {
                Name = conv.ConversationName,
                Subject = conv.Subject?.SubjectName,
                Tags = conv.ConversationTags.Where(t => t.Tag != null).Select(t => t.Tag!.TagName).ToList()
            };

            var vec = await _embed.ConvVectorAsync(req, ct);

            await _convStore.UpsertAsync(new Domain.Entities.ConversationVector
            {
                Id = Guid.NewGuid(),
                ConversationId = convId,
                EmbeddingJson = System.Text.Json.JsonSerializer.Serialize(vec),
                CalculatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            }, ct);

            _log.LogInformation("Updated conv vector: {ConvId}", convId);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to update conv vector: {ConvId}", convId);
        }
    }

    public async Task InvalidateUserRecommendationsCacheAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            await _cache.RemoveByPatternAsync($"rec:{userId}:*", ct);
        }
        catch { /* ignore */ }
    }
}
