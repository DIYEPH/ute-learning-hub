using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class DocumentFile
{
    public Guid FileId { get; set; }
    public Guid DocumentId { get; set; }
    public File File { get; set; } = default!;
    public Document Document { get; set; } = default!;
}
