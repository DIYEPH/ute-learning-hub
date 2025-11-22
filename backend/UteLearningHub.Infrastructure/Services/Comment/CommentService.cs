using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Comment;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.Comment;

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _dbContext;

    public CommentService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Dictionary<Guid, CommentAuthorInfo>> GetCommentAuthorsAsync(
        IEnumerable<Guid> userIds, 
        CancellationToken cancellationToken = default)
    {
        var userIdList = userIds.Distinct().ToList();
        
        if (!userIdList.Any())
            return new Dictionary<Guid, CommentAuthorInfo>();

        var users = await _dbContext.Users
            .Where(u => userIdList.Contains(u.Id))
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.AvatarUrl
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return users.ToDictionary(
            u => u.Id,
            u => new CommentAuthorInfo
            {
                FullName = u.FullName,
                AvatarUrl = u.AvatarUrl
            });
    }
}
