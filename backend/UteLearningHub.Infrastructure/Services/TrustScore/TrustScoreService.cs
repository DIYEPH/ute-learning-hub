using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UteLearningHub.Application.Services.TrustScore;
using UteLearningHub.Application.Services.User;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.TrustScore;

public class TrustScoreService : ITrustScoreService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IUserService _userService;
    private readonly ILogger<TrustScoreService> _logger;


    public TrustScoreService(
        ApplicationDbContext dbContext,
        IUserService userService,
        ILogger<TrustScoreService> logger)
    {
        _dbContext = dbContext;
        _userService = userService;
        _logger = logger;
    }

    public async Task AddTrustScoreAsync(Guid userId, int points, string reason, Guid? entityId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User not found for userId {UserId}", userId);
                return;
            }

            var oldTrustScore = user.TrustScore;
            var newTrustScore = oldTrustScore + points;

            await _userService.UpdateTrustScoreAsync(userId, newTrustScore, reason, entityId, cancellationToken);
            _logger.LogInformation("Updated trust score for user {UserId}: {OldScore} -> {NewScore} ({Delta}) - Reason: {Reason}, EntityId: {EntityId}",
                userId, oldTrustScore, newTrustScore, points, reason, entityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating trust score for user {UserId}", userId);
            throw;
        }
    }

    public async Task RevertTrustScoreByEntityAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find all trust history entries related to this entity
            var historyEntries = await _dbContext.UserTrustHistories
                .Where(h => h.EntityId == entityId)
                .ToListAsync(cancellationToken);

            if (!historyEntries.Any())
            {
                _logger.LogInformation("No trust history found for entityId {EntityId}", entityId);
                return;
            }

            // Group by user and calculate total points to revert
            var userPointsToRevert = historyEntries
                .GroupBy(h => h.UserId)
                .Select(g => new { UserId = g.Key, TotalPoints = g.Sum(h => h.Score) })
                .ToList();

            foreach (var userPoints in userPointsToRevert)
            {
                // Revert points by adding negative of what was awarded
                var pointsToDeduct = -(int)userPoints.TotalPoints;
                await AddTrustScoreAsync(userPoints.UserId, pointsToDeduct, $"Hoàn trả điểm do xóa tài liệu", null, cancellationToken);
                _logger.LogInformation("Reverted {Points} trust points for user {UserId} due to entity {EntityId} deletion",
                    pointsToDeduct, userPoints.UserId, entityId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reverting trust score for entityId {EntityId}", entityId);
            throw;
        }
    }

    public async Task<int> GetTrustScoreAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user?.TrustScore ?? 0;
    }

}

