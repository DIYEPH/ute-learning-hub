using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Features.Author.Queries.GetAuthorById;
using UteLearningHub.Application.Services.Author;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.Author;

public class AuthorQueryService : IAuthorQueryService
{
    private readonly ApplicationDbContext _dbContext;

    public AuthorQueryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuthorDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Authors
            .AsNoTracking()
            .Where(a => a.Id == id && !a.IsDeleted && a.ReviewStatus == ReviewStatus.Approved)
            .Select(a => new AuthorDetailDto
            {
                Id = a.Id,
                FullName = a.FullName,
                Description = a.Description,
                DocumentCount = a.DocumentAuthors.Count
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
