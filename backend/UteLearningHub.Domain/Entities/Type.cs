using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Type : SoftDeletableEntity<Guid>, IAggregateRoot, IAuditable
{
    public string TypeName { get; set; } = default!;
    public ICollection<Document> Documents { get; set; } = [];
    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
}
