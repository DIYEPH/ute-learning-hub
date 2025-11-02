namespace UteLearningHub.Domain.Entities.Base;

public interface ISoftDelete<TKey>
{
    bool IsDeleted { get; set; }
    DateTimeOffset DeletedAt { get; set; }
    TKey DeletedBy { get; set; }
}
