using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.Recommendation;

public class VectorMaintenanceService(
    IEmbeddingService embed,
    IProfileVectorStore userStore,
    IConversationVectorStore convStore,
    IUserDataRepository userData,
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ILogger<VectorMaintenanceService> log) : IVectorMaintenanceService
{
    // Cập nhật vector cho user dựa trên hành vi
    public async Task UpdateUserVectorAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var data = await userData.GetUserBehaviorTextDataAsync(userId, ct);
            if (data == null) return;

            var req = new UserVectorRequest
            {
                Subjects = data.SubjectScores.Select(x => x.Name).ToList(),
                SubjectWeights = data.SubjectScores.Select(x => (float)x.Score).ToList(),
                Tags = data.TagScores.Select(x => x.Name).ToList(),
                TagWeights = data.TagScores.Select(x => (float)x.Score).ToList()
            };

            var vec = await embed.UserVectorAsync(req, ct);

            await userStore.UpsertAsync(new ProfileVector
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EmbeddingJson = JsonSerializer.Serialize(vec),
                CalculatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            }, ct);

            log.LogInformation("Updated user vector: {UserId}", userId);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to update user vector: {UserId}", userId);
        }
    }

    // Cập nhật vector cho conversation
    public async Task UpdateConversationVectorAsync(Guid convId, CancellationToken ct = default)
    {
        try
        {
            await using var db = await dbFactory.CreateDbContextAsync(ct);

            var conv = await db.Conversations
                .Include(c => c.Subject)
                .Include(c => c.ConversationTags).ThenInclude(t => t.Tag)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == convId && !c.IsDeleted, ct);

            if (conv == null) return;

            var req = new ConvVectorRequest
            {
                Name = conv.ConversationName,
                Subject = conv.Subject?.SubjectName,
                Tags = conv.ConversationTags.Where(t => t.Tag != null).Select(t => t.Tag!.TagName).ToList()
            };

            var vec = await embed.ConvVectorAsync(req, ct);

            await convStore.UpsertAsync(new ConversationVector
            {
                Id = Guid.NewGuid(),
                ConversationId = convId,
                EmbeddingJson = JsonSerializer.Serialize(vec),
                CalculatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            }, ct);

            log.LogInformation("Updated conv vector: {ConvId}", convId);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to update conv vector: {ConvId}", convId);
        }
    }
}
