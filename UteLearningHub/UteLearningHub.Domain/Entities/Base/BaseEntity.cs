namespace UteLearningHub.Domain.Entities.Base;

public abstract class BaseEntity<TKey> : IHasKey<TKey>, ITrackable, ISoftDelete
{
    public TKey Id { get; set; } = default!;

    public byte[] RowVersion { get; set;} = default!;
    public DateTimeOffset CreatedAt { get; set;}
    public DateTimeOffset? UpdatedAt { get;set;}

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedById { get; set; } = default!;
}