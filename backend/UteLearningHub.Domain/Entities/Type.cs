using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Type : BaseEntity<Guid>, IAggregateRoot
{
    public string TypeName { get; set; } = default!;
    public ICollection<Document> Documents { get; set; } = [];
}
