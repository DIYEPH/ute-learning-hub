using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Document : BaseEntity<Guid>, IAggregateRoot, IAuditable<Guid>, IReviewable<Guid>
{
    public Guid SubjectId { get; set; }
    public Guid TypeId { get; set; }
    public string Description { get; set; } = default!;
    public string DocumentName { get; set; } = default!;
    public string AuthorName { get; set; } = default!;
    public string DescriptionAuthor { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public bool IsDownload { get; set; } = true;
    public VisibilityStatus Visibility { get; set; } = VisibilityStatus.Public;
    public Subject Subject { get; set; } = default!; 
    public Type Type { get; set; } = default!;
    public ICollection<DocumentTag> DocumentTags { get; set; } = [];
    public ICollection<DocumentFile> DocumentFiles { get; set; } = [];
    public ICollection<Report> Reports { get; set; } = [];
    public ICollection<DocumentReview> Reviews { get; set; } = [];

    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }

    public Guid ReviewedBy { get; set; }
    public DateTimeOffset ReviewedAt { get; set; }
    public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.Approved;
    public string ReviewNote { get; set; } = default!;
}
