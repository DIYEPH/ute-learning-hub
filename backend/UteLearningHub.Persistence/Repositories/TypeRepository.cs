using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;
using DomainType = UteLearningHub.Domain.Entities.Type;

namespace UteLearningHub.Persistence.Repositories;

public class TypeRepository : Repository<DomainType, Guid>, ITypeRepository
{
    public TypeRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext, dateTimeProvider)
    {
    }
}
