using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence;

namespace UteLearningHub.Api.BackgroundServices;

/// <summary>
/// Background service to periodically update all vectors
/// </summary>
public class VectorUpdateService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<VectorUpdateService> _log;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);
    private readonly TimeSpan _delay = TimeSpan.FromMinutes(5);

    public VectorUpdateService(IServiceProvider sp, ILogger<VectorUpdateService> log)
    {
        _sp = sp;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _log.LogInformation("VectorUpdateService started");
        await Task.Delay(_delay, ct);

        while (!ct.IsCancellationRequested)
        {
            try { await UpdateAllAsync(ct); }
            catch (Exception ex) { _log.LogError(ex, "Vector update failed"); }
            await Task.Delay(_interval, ct);
        }
    }

    private async Task UpdateAllAsync(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userData = scope.ServiceProvider.GetRequiredService<IUserDataRepository>();
        var embed = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
        var userStore = scope.ServiceProvider.GetRequiredService<IProfileVectorStore>();
        var convStore = scope.ServiceProvider.GetRequiredService<IConversationVectorStore>();

        _log.LogInformation("Starting vector update");

        // Update user vectors
        var userIds = await db.Users.Where(u => !u.IsDeleted).Select(u => u.Id).ToListAsync(ct);
        var userCount = 0;

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

                await userStore.UpsertAsync(new Domain.Entities.ProfileVector
                {
                    Id = Guid.NewGuid(),
                    UserId = uid,
                    EmbeddingJson = System.Text.Json.JsonSerializer.Serialize(vec),
                    CalculatedAt = DateTimeOffset.UtcNow,
                    IsActive = true
                }, ct);

                userCount++;
            }
            catch (Exception ex) { _log.LogWarning(ex, "Failed user: {Id}", uid); }
        }

        // Update conversation vectors
        var convs = await db.Conversations
            .Include(c => c.Subject)
            .Include(c => c.ConversationTags).ThenInclude(t => t.Tag)
            .Where(c => !c.IsDeleted)
            .ToListAsync(ct);

        var convCount = 0;

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

                await convStore.UpsertAsync(new Domain.Entities.ConversationVector
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conv.Id,
                    EmbeddingJson = System.Text.Json.JsonSerializer.Serialize(vec),
                    CalculatedAt = DateTimeOffset.UtcNow,
                    IsActive = true
                }, ct);

                convCount++;
            }
            catch (Exception ex) { _log.LogWarning(ex, "Failed conv: {Id}", conv.Id); }
        }

        _log.LogInformation("Updated {Users} users, {Convs} conversations", userCount, convCount);
    }
}
