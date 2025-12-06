using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence;

namespace UteLearningHub.Api.BackgroundServices;

public class VectorUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VectorUpdateService> _logger;
    private readonly TimeSpan _updateInterval;
    private readonly TimeSpan _initialDelay;

    public VectorUpdateService(
        IServiceProvider serviceProvider,
        ILogger<VectorUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _updateInterval = TimeSpan.FromHours(1); // Update vectors mỗi giờ
        _initialDelay = TimeSpan.FromMinutes(5); // Delay 5 phút khi khởi động
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("VectorUpdateService started, waiting {InitialDelay} before first update", _initialDelay);

        // Initial delay to let the system stabilize
        await Task.Delay(_initialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateVectorsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating vectors");
            }

            await Task.Delay(_updateInterval, stoppingToken);
        }

        _logger.LogInformation("VectorUpdateService stopped");
    }

    private async Task UpdateVectorsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userDataRepository = scope.ServiceProvider.GetRequiredService<IUserDataRepository>();
        var vectorCalculationService = scope.ServiceProvider.GetRequiredService<IVectorCalculationService>();
        var profileVectorStore = scope.ServiceProvider.GetRequiredService<IProfileVectorStore>();
        var conversationVectorStore = scope.ServiceProvider.GetRequiredService<IConversationVectorStore>();

        _logger.LogInformation("Starting vector update process (behavior-based)");

        // Update user vectors using behavior-based calculation
        var userIds = await dbContext.Users
            .Where(u => !u.IsDeleted)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var userUpdateCount = 0;
        foreach (var userId in userIds)
        {
            try
            {
                var behaviorData = await userDataRepository.GetUserBehaviorDataAsync(userId, cancellationToken);
                if (behaviorData == null)
                    continue;

                var vector = vectorCalculationService.CalculateUserVectorFromBehavior(
                    behaviorData.FacultyScores,
                    behaviorData.TypeScores,
                    behaviorData.TagScores,
                    100);

                var profileVector = new UteLearningHub.Domain.Entities.ProfileVector
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    VectorType = UteLearningHub.Domain.Constaints.Enums.ProfileVectorType.UserSubject,
                    VectorDimension = 100,
                    EmbeddingJson = System.Text.Json.JsonSerializer.Serialize(vector),
                    CalculatedAt = DateTimeOffset.UtcNow,
                    IsActive = true
                };

                await profileVectorStore.UpsertAsync(profileVector, cancellationToken);
                userUpdateCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update vector for user {UserId}", userId);
            }
        }

        // Update conversation vectors using Faculty-based calculation
        var conversations = await dbContext.Conversations
            .Include(c => c.Subject)
                .ThenInclude(s => s!.SubjectMajors)
                    .ThenInclude(sm => sm.Major)
            .Include(c => c.ConversationTags)
            .Where(c => !c.IsDeleted)
            .ToListAsync(cancellationToken);

        var conversationUpdateCount = 0;
        foreach (var conversation in conversations)
        {
            try
            {
                // Get FacultyIds from Subject → SubjectMajors → Major
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

                var vector = vectorCalculationService.CalculateConversationVector(
                    facultyIds.Distinct().ToList(),
                    tagIds,
                    100);

                var conversationVector = new UteLearningHub.Domain.Entities.ConversationVector
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversation.Id,
                    SubjectId = conversation.SubjectId,
                    VectorType = UteLearningHub.Domain.Constaints.Enums.ProfileVectorType.ConversationTopic,
                    VectorDimension = 100,
                    EmbeddingJson = System.Text.Json.JsonSerializer.Serialize(vector),
                    CalculatedAt = DateTimeOffset.UtcNow,
                    IsActive = true
                };

                await conversationVectorStore.UpsertAsync(conversationVector, cancellationToken);
                conversationUpdateCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update vector for conversation {ConversationId}", conversation.Id);
            }
        }

        _logger.LogInformation(
            "Vector update completed (behavior-based). Updated {UserCount} user vectors and {ConversationCount} conversation vectors",
            userUpdateCount,
            conversationUpdateCount);
    }
}
