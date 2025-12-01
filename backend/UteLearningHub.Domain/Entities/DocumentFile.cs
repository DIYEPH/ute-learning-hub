using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class DocumentFile : BaseEntity<Guid>, IAuditable
{
    public Guid DocumentId { get; set; }
    public Guid FileId { get; set; }

    public string? Title { get; set; }
    public int? TotalPages { get; set; }
    public bool IsPrimary { get; set; } = false;
    public int Order { get; set; } = 0;

    public Document Document { get; set; } = default!;
    public File File { get; set; } = default!;
    public Guid? CoverFileId { get; set; }
    public File? CoverFile { get; set; }
    public ICollection<Comment> Comments { get; set; } = [];

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
}


