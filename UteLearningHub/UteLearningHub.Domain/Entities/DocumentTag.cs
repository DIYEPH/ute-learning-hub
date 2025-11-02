namespace UteLearningHub.Domain.Entities;

public class DocumentTag
{
    public Guid TagId { get; set; }
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = default!;
    public Tag Tag { get; set; } = default!;
}
