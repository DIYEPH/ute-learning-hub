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

    public async Task AddTrustScoreAsync(Guid userId, int points, string reason, CancellationToken cancellationToken = default)
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

            await _userService.UpdateTrustScoreAsync(userId, newTrustScore, reason, cancellationToken);
            _logger.LogInformation("Updated trust score for user {UserId}: {OldScore} -> {NewScore} ({Delta}) - Reason: {Reason}",
                userId, oldTrustScore, newTrustScore, points, reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating trust score for user {UserId}", userId);
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

