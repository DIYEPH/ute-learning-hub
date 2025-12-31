using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.Recommendation;

public class UserDataRepository : IUserDataRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    // Score weights
    private const int DocumentCreatedScore = 3;
    private const int ConversationJoinedScore = 2;
    private const int UsefulVoteScore = 1;

    public UserDataRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    private static void AddTextScore(Dictionary<string, int> scores, string name, int score)
    {
        if (scores.ContainsKey(name))
            scores[name] += score;
        else
            scores[name] = score;
    }

    public async Task<UserBehaviorTextDataDto?> GetUserBehaviorTextDataAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Check if user exists and get Major info
        var user = await dbContext.Users
            .Include(u => u.Major)
                .ThenInclude(m => m!.Faculty)
            .AsNoTracking()
            .Where(u => u.Id == userId && !u.IsDeleted)
            .Select(u => new { u.Id, u.Major })
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
            return null;

        var subjectScores = new Dictionary<string, int>();
        var tagScores = new Dictionary<string, int>();

        const int MajorScore = 5;
        if (user.Major != null)
            AddTextScore(subjectScores, user.Major.MajorName, MajorScore);

        // 1. Documents created by user
        var documents = await dbContext.Documents
            .Include(d => d.Subject)
            .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
            .AsNoTracking()
            .Where(d => d.CreatedById == userId && !d.IsDeleted && d.DocumentFiles.Any())
            .ToListAsync(cancellationToken);

        foreach (var doc in documents)
        {
            // Subject score
            if (doc.Subject != null)
                AddTextScore(subjectScores, doc.Subject.SubjectName, DocumentCreatedScore);

            // Tag scores
            foreach (var dt in doc.DocumentTags)
                if (dt.Tag != null)
                    AddTextScore(tagScores, dt.Tag.TagName, DocumentCreatedScore);
        }

        // 2. Conversations user is a member of
        var conversationMembers = await dbContext.Set<Domain.Entities.ConversationMember>()
            .Include(cm => cm.Conversation)
                .ThenInclude(c => c.Subject)
            .Include(cm => cm.Conversation)
                .ThenInclude(c => c.ConversationTags)
                    .ThenInclude(ct => ct.Tag)
            .AsNoTracking()
            .Where(cm => cm.UserId == userId && !cm.IsDeleted && !cm.Conversation.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var cm in conversationMembers)
        {
            var conv = cm.Conversation;

            // Subject score
            if (conv.Subject != null)
                AddTextScore(subjectScores, conv.Subject.SubjectName, ConversationJoinedScore);

            // Tag scores
            foreach (var ct in conv.ConversationTags)
                if (ct.Tag != null)
                    AddTextScore(tagScores, ct.Tag.TagName, ConversationJoinedScore);
        }

        // 3. Document reviews
        var reviews = await dbContext.Set<Domain.Entities.DocumentReview>()
            .Include(dr => dr.Document)
                .ThenInclude(d => d.Subject)
            .Include(dr => dr.Document)
                .ThenInclude(d => d.DocumentTags)
                    .ThenInclude(dt => dt.Tag)
            .AsNoTracking()
            .Where(dr => dr.CreatedById == userId && !dr.Document.IsDeleted)
            .ToListAsync(cancellationToken);

        // Chỉ tính Useful reviews (Not Useful không phản ánh sở thích, chỉ đánh giá chất lượng tài liệu)
        foreach (var review in reviews.Where(r => r.DocumentReviewType == DocumentReviewType.Useful))
        {
            var doc = review.Document;

            // Subject score
            if (doc.Subject != null)
                AddTextScore(subjectScores, doc.Subject.SubjectName, UsefulVoteScore);

            // Tag scores
            foreach (var dt in doc.DocumentTags)
                if (dt.Tag != null)
                    AddTextScore(tagScores, dt.Tag.TagName, UsefulVoteScore);
        }

        return new UserBehaviorTextDataDto(
            userId,
            subjectScores.Where(x => x.Value > 0).Select(x => new TextScoreItem(x.Key, x.Value)).ToList(),
            tagScores.Where(x => x.Value > 0).Select(x => new TextScoreItem(x.Key, x.Value)).ToList()
        );
    }
}
