namespace UteLearningHub.Domain.Entities.Base;

public interface ITrackable
{
    byte[]? RowVersion { get; set; }
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
}
