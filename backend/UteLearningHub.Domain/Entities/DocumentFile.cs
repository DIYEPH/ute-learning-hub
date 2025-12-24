using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class DocumentFile : SoftDeletableEntity<Guid>, IAuditable
{
    public Guid DocumentId { get; set; }
    public Guid FileId { get; set; }

    public string? Title { get; set; }
    public int? TotalPages { get; set; }
    public int Order { get; set; } = 0;
    public int ViewCount { get; set; } = 0;  

    public Document Document { get; set; } = default!;
    public File File { get; set; } = default!;
    public Guid? CoverFileId { get; set; }
    public File? CoverFile { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Approved;
    
    // Review info
    public Guid? ReviewedById { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? ReviewNote { get; set; }
    
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Report> Reports { get; set; } = [];

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
}