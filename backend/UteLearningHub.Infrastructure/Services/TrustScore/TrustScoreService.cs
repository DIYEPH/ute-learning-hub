using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UteLearningHub.Application.Services.TrustScore;
using UteLearningHub.Application.Services.User;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.TrustScore;

public class TrustScoreService(
    ApplicationDbContext dbContext,
    IUserService userService,
    ILogger<TrustScoreService> logger) : ITrustScoreService
{
    public async Task AddTrustScoreAsync(
        Guid userId, int points, string reason,
        Guid? entityId = null, TrustEntityType? entityType = null,
        CancellationToken ct = default)
    {
        try
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user == null)
            {
                logger.LogWarning("User not found for userId {UserId}", userId);
                return;
            }

            var oldScore = user.TrustScore;
            var newScore = oldScore + points;
            await userService.UpdateTrustScoreAsync(userId, newScore, reason, entityId, entityType, ct);
            logger.LogInformation("Updated trust score for user {UserId}: {OldScore} -> {NewScore}", userId, oldScore, newScore);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating trust score for user {UserId}", userId);
            throw;
        }
    }

    public async Task RevertTrustScoreByEntityAsync(
        Guid entityId, TrustEntityType? entityType = null,
        CancellationToken ct = default)
    {
        try
        {
            var query = dbContext.UserTrustHistories.Where(h => h.EntityId == entityId);
            if (entityType.HasValue)
                query = query.Where(h => h.EntityType == entityType);

            var historyEntries = await query.ToListAsync(ct);
            if (historyEntries.Count == 0)
            {
                logger.LogInformation("No trust history found for entityId {EntityId}, entityType {EntityType}", entityId, entityType);
                return;
            }

            var userPointsToRevert = historyEntries
                .GroupBy(h => h.UserId)
                .Select(g => new { UserId = g.Key, TotalPoints = g.Sum(h => h.Score) })
                .ToList();

            foreach (var up in userPointsToRevert)
            {
                var pointsToDeduct = -(int)up.TotalPoints;
                await AddTrustScoreAsync(up.UserId, pointsToDeduct, "Hoàn trả điểm do xóa nội dung", null, null, ct);
                logger.LogInformation("Reverted {Points} - user {UserId} - {EntityId} deletion", pointsToDeduct, up.UserId, entityId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reverting trust score for entityId {EntityId}", entityId);
            throw;
        }
    }
}