using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;

namespace UteLearningHub.Persistence.Repositories;

public class AuthorRepository : Repository<Author, Guid>, IAuthorRepository
{
    public AuthorRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
        : base(dbContext, dateTimeProvider)
    {
    }
}


