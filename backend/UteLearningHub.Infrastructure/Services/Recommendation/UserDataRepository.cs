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
    private const int NotUsefulVoteScore = -1;

    public UserDataRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<UserBehaviorDataDto?> GetUserBehaviorDataAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Check if user exists
        var userExists = await dbContext.Users.AnyAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
        if (!userExists)
            return null;

        // Aggregate scores
        var facultyScores = new Dictionary<Guid, int>();
        var typeScores = new Dictionary<Guid, int>();
        var tagScores = new Dictionary<Guid, int>();

        // 1. Documents created by user
        var documents = await dbContext.Documents
            .Include(d => d.Subject)
                .ThenInclude(s => s!.SubjectMajors)
                    .ThenInclude(sm => sm.Major)
            .Include(d => d.DocumentTags)
            .AsNoTracking()
            .Where(d => d.CreatedById == userId && !d.IsDeleted && d.DocumentFiles.Any())
            .ToListAsync(cancellationToken);

        foreach (var doc in documents)
        {
            // Type score
            AddScore(typeScores, doc.TypeId, DocumentCreatedScore);

            // Faculty score (only if has Subject)
            if (doc.Subject != null)
            {
                foreach (var sm in doc.Subject.SubjectMajors)
                {
                    if (sm.Major != null && !sm.Major.IsDeleted)
                    {
                        AddScore(facultyScores, sm.Major.FacultyId, DocumentCreatedScore);
                    }
                }
            }

            // Tag scores
            foreach (var dt in doc.DocumentTags)
            {
                AddScore(tagScores, dt.TagId, DocumentCreatedScore);
            }
        }

        // 2. Conversations user is a member of (active only)
        var conversationMembers = await dbContext.Set<Domain.Entities.ConversationMember>()
            .Include(cm => cm.Conversation)
                .ThenInclude(c => c.Subject)
                    .ThenInclude(s => s!.SubjectMajors)
                        .ThenInclude(sm => sm.Major)
            .Include(cm => cm.Conversation)
                .ThenInclude(c => c.ConversationTags)
            .AsNoTracking()
            .Where(cm => cm.UserId == userId && !cm.IsDeleted && !cm.Conversation.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var cm in conversationMembers)
        {
            var conv = cm.Conversation;

            // Faculty score (only if has Subject)
            if (conv.Subject != null)
            {
                foreach (var sm in conv.Subject.SubjectMajors)
                {
                    if (sm.Major != null && !sm.Major.IsDeleted)
                    {
                        AddScore(facultyScores, sm.Major.FacultyId, ConversationJoinedScore);
                    }
                }
            }

            // Tag scores
            foreach (var ct in conv.ConversationTags)
            {
                AddScore(tagScores, ct.TagId, ConversationJoinedScore);
            }
        }

        // 3. Document reviews (Useful/NotUseful votes)
        var reviews = await dbContext.Set<Domain.Entities.DocumentReview>()
            .Include(dr => dr.Document)
                .ThenInclude(d => d.Subject)
                    .ThenInclude(s => s!.SubjectMajors)
                        .ThenInclude(sm => sm.Major)
            .Include(dr => dr.Document)
                .ThenInclude(d => d.DocumentTags)
            .AsNoTracking()
            .Where(dr => dr.CreatedById == userId && !dr.IsDeleted && !dr.Document.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var review in reviews)
        {
            var doc = review.Document;
            var score = review.DocumentReviewType == DocumentReviewType.Useful 
                ? UsefulVoteScore 
                : NotUsefulVoteScore;

            // Type score
            AddScore(typeScores, doc.TypeId, score);

            // Faculty score (only if has Subject)
            if (doc.Subject != null)
            {
                foreach (var sm in doc.Subject.SubjectMajors)
                {
                    if (sm.Major != null && !sm.Major.IsDeleted)
                    {
                        AddScore(facultyScores, sm.Major.FacultyId, score);
                    }
                }
            }

            // Tag scores
            foreach (var dt in doc.DocumentTags)
            {
                AddScore(tagScores, dt.TagId, score);
            }
        }

        return new UserBehaviorDataDto(
            userId,
            facultyScores.Where(x => x.Value > 0).Select(x => new ScoreItem(x.Key, x.Value)).ToList(),
            typeScores.Where(x => x.Value > 0).Select(x => new ScoreItem(x.Key, x.Value)).ToList(),
            tagScores.Where(x => x.Value > 0).Select(x => new ScoreItem(x.Key, x.Value)).ToList()
        );
    }

    private static void AddScore(Dictionary<Guid, int> scores, Guid id, int score)
    {
        if (scores.ContainsKey(id))
            scores[id] += score;
        else
            scores[id] = score;
    }
}
