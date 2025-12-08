namespace UteLearningHub.Domain.Entities.Base;

public abstract class SoftDeletableEntity<TKey> : BaseEntity<TKey>, ISoftDelete
{
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedById { get; set; }
}
