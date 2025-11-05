using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class File : BaseEntity<Guid>, IAggregateRoot, IAuditable
{
    public string FileName { get; set; } = default!;
    public string FileUrl { get; set; } = default!;
    public double FileSize { get; set; }
    public string FileType { get; set; } = default!;
    public ICollection<MessageFile> MessageFiles { get; set; } = [];
    public ICollection<DocumentFile> DocumentFiles { get; set; } = [];

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
}
