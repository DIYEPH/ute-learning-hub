namespace UteLearningHub.Domain.Entities.Base;

public interface IAuditable
{
    Guid CreatedById { get; set; }
    Guid? UpdatedById { get; set; }
}
