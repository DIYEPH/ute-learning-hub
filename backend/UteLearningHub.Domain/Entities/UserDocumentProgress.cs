using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class UserDocumentProgress : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid? DocumentFileId { get; set; }

    public int LastPage { get; set; }
    public int? TotalPages { get; set; }
    public DateTimeOffset LastAccessedAt { get; set; }

    public Document Document { get; set; } = default!;
    public DocumentFile? DocumentFile { get; set; }

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
}


