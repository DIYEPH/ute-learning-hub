namespace UteLearningHub.Domain.Entities.Base;

public abstract class BaseEntity<TKey> : IHasKey<TKey>, ITrackable
{
    public TKey Id { get; set; } = default!;

    public byte[] RowVersion { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}