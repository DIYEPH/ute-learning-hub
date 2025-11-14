namespace UteLearningHub.Domain.Entities.Base;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
    Guid? DeletedById { get; set; }
}
