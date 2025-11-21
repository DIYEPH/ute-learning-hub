using UteLearningHub.Domain.Repositories.Base;
using DomainFile = UteLearningHub.Domain.Entities.File;

namespace UteLearningHub.Domain.Repositories;

public interface IFileRepository : IRepository<DomainFile, Guid>
{

}