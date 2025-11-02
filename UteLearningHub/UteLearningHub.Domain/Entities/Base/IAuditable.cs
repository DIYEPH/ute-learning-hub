namespace UteLearningHub.Domain.Entities.Base;

public interface IAuditable<TKey>
{
    TKey CreatedBy { get; set; }
    TKey UpdatedBy { get; set; }
}
