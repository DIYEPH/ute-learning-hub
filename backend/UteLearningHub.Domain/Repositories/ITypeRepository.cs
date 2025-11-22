using UteLearningHub.Domain.Repositories.Base;
using DomainType = UteLearningHub.Domain.Entities.Type;

namespace UteLearningHub.Domain.Repositories;

public interface ITypeRepository : IRepository<DomainType, Guid>
{
}
