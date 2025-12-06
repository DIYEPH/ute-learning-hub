namespace UteLearningHub.Domain.Entities.Base;

public static class SoftDeleteExtensions
{
    public static void MarkAsDeleted(this ISoftDelete entity, Guid? deletedById = null, DateTimeOffset? deletedAt = null)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = deletedAt ?? DateTimeOffset.UtcNow;
        entity.DeletedById = deletedById;
    }

    public static void Restore(this ISoftDelete entity)
    {
        entity.IsDeleted = false;
        entity.DeletedAt = null;
        entity.DeletedById = null;
    }
}
