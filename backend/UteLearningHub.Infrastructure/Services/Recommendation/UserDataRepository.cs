using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.Recommendation;

public class UserDataRepository(IDbContextFactory<ApplicationDbContext> dbFactory) : IUserDataRepository
{
    private const int MajorScore = 2;
    private const int DocumentCreatedScore = 2;
    private const int ConversationJoinedScore = 1;
    private const int UsefulVoteScore = 1;

    public async Task<UserBehaviorTextDataDto?> GetUserBehaviorTextDataAsync(Guid userId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        // Lấy user và major
        var user = await db.Users
            .Include(u => u.Major)
            .AsNoTracking()
            .Where(u => u.Id == userId && !u.IsDeleted)
            .Select(u => new { u.Id, u.Major })
            .FirstOrDefaultAsync(ct);

        if (user == null) return null;

        var subjectScores = new Dictionary<string, int>();
        var tagScores = new Dictionary<string, int>();

        if (user.Major != null)
            AddScore(subjectScores, user.Major.MajorName, MajorScore);

        // Tài liệu đã tạo
        var documents = await db.Documents
            .Include(d => d.Subject)
            .Include(d => d.DocumentTags).ThenInclude(dt => dt.Tag)
            .AsNoTracking()
            .Where(d => d.CreatedById == userId && !d.IsDeleted && d.DocumentFiles.Any())
            .ToListAsync(ct);

        foreach (var doc in documents)
        {
            if (doc.Subject != null)
                AddScore(subjectScores, doc.Subject.SubjectName, DocumentCreatedScore);
            foreach (var dt in doc.DocumentTags.Where(t => t.Tag != null))
                AddScore(tagScores, dt.Tag!.TagName, DocumentCreatedScore);
        }

        // Nhóm đã tham gia
        var members = await db.Set<ConversationMember>()
            .Include(cm => cm.Conversation).ThenInclude(c => c.Subject)
            .Include(cm => cm.Conversation).ThenInclude(c => c.ConversationTags).ThenInclude(ct => ct.Tag)
            .AsNoTracking()
            .Where(cm => cm.UserId == userId && !cm.IsDeleted && !cm.Conversation.IsDeleted)
            .ToListAsync(ct);

        foreach (var cm in members)
        {
            var conv = cm.Conversation;
            if (conv.Subject != null)
                AddScore(subjectScores, conv.Subject.SubjectName, ConversationJoinedScore);
            foreach (var ctg in conv.ConversationTags.Where(t => t.Tag != null))
                AddScore(tagScores, ctg.Tag!.TagName, ConversationJoinedScore);
        }

        // Đánh giá tl (chỉ Useful)
        var reviews = await db.Set<DocumentReview>()
            .Include(dr => dr.Document).ThenInclude(d => d.Subject)
            .Include(dr => dr.Document).ThenInclude(d => d.DocumentTags).ThenInclude(dt => dt.Tag)
            .AsNoTracking()
            .Where(dr => dr.CreatedById == userId && !dr.Document.IsDeleted && dr.DocumentReviewType == DocumentReviewType.Useful)
            .ToListAsync(ct);

        foreach (var review in reviews)
        {
            var doc = review.Document;
            if (doc.Subject != null)
                AddScore(subjectScores, doc.Subject.SubjectName, UsefulVoteScore);
            foreach (var dt in doc.DocumentTags.Where(t => t.Tag != null))
                AddScore(tagScores, dt.Tag!.TagName, UsefulVoteScore);
        }

        return new UserBehaviorTextDataDto(
            userId,
            subjectScores.Select(x => new TextScoreItem(x.Key, x.Value)).ToList(),
            tagScores.Select(x => new TextScoreItem(x.Key, x.Value)).ToList()
        );
    }

    private static void AddScore(Dictionary<string, int> scores, string name, int score)
        => scores[name] = scores.GetValueOrDefault(name) + score;
}
