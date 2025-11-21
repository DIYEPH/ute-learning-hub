using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;
using DomainFile = UteLearningHub.Domain.Entities.File;

namespace UteLearningHub.Persistence.Repositories;

public class FileRepository : Repository<DomainFile, Guid>, IFileRepository
{
    public FileRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext, dateTimeProvider)
    {
    }
}
