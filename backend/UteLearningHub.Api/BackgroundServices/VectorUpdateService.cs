using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence;

namespace UteLearningHub.Api.BackgroundServices;

// chạy mỗi giờ
public class VectorUpdateService(IServiceProvider sp, ILogger<VectorUpdateService> log) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _delay = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await Task.Delay(_delay, ct);
        while (!ct.IsCancellationRequested)
        {
            try { await UpdateAllAsync(ct); }
            catch (Exception ex) { log.LogError(ex, "Vector update failed"); }
            await Task.Delay(_interval, ct);
        }
    }

    private async Task UpdateAllAsync(CancellationToken ct)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userData = scope.ServiceProvider.GetRequiredService<IUserDataRepository>();
        var embed = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
        var userStore = scope.ServiceProvider.GetRequiredService<IProfileVectorStore>();
        var convStore = scope.ServiceProvider.GetRequiredService<IConversationVectorStore>();

        var userIds = await db.Users.Where(u => !u.IsDeleted).Select(u => u.Id).ToListAsync(ct);
        foreach (var uid in userIds)
        {
            try
            {
                var data = await userData.GetUserBehaviorTextDataAsync(uid, ct);
                if (data == null) continue;

                var vec = await embed.UserVectorAsync(new UserVectorRequest
                {
                    Subjects = data.SubjectScores.Select(x => x.Name).ToList(),
                    SubjectWeights = data.SubjectScores.Select(x => (float)x.Score).ToList(),
                    Tags = data.TagScores.Select(x => x.Name).ToList(),
                    TagWeights = data.TagScores.Select(x => (float)x.Score).ToList()
                }, ct);

                await userStore.UpsertAsync(new ProfileVector
                {
                    Id = Guid.NewGuid(),
                    UserId = uid,
                    EmbeddingJson = JsonSerializer.Serialize(vec),
                    CalculatedAt = DateTimeOffset.UtcNow,
                    IsActive = true
                }, ct);
            }
            catch (Exception ex) { log.LogWarning(ex, "Failed user: {Id}", uid); }
        }

        var convs = await db.Conversations
            .Include(c => c.Subject)
            .Include(c => c.ConversationTags).ThenInclude(t => t.Tag)
            .Where(c => !c.IsDeleted)
            .ToListAsync(ct);

        foreach (var conv in convs)
        {
            try
            {
                var vec = await embed.ConvVectorAsync(new ConvVectorRequest
                {
                    Name = conv.ConversationName,
                    Subject = conv.Subject?.SubjectName,
                    Tags = conv.ConversationTags.Where(t => t.Tag != null).Select(t => t.Tag!.TagName).ToList()
                }, ct);

                await convStore.UpsertAsync(new ConversationVector
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conv.Id,
                    EmbeddingJson = JsonSerializer.Serialize(vec),
                    CalculatedAt = DateTimeOffset.UtcNow,
                    IsActive = true
                }, ct);
            }
            catch (Exception ex) { log.LogWarning(ex, "Failed conv: {Id}", conv.Id); }
        }
    }
}