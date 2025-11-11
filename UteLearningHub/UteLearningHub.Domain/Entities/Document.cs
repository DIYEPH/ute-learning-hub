using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Document : BaseEntity<Guid>, IAggregateRoot, IAuditable, IReviewable
{
    public Guid SubjectId { get; set; }
    public Guid TypeId { get; set; }
    public string Description { get; set; } = default!;
    public string DocumentName { get; set; } = default!;
    public string AuthorName { get; set; } = default!;
    public string DescriptionAuthor { get; set; } = default!;
    public string NormalizedName { get; set; } = default!;
    public bool IsDownload { get; set; } = true;
    public VisibilityStatus Visibility { get; set; } = VisibilityStatus.Public;
    public Subject Subject { get; set; } = default!;
    public Type Type { get; set; } = default!;
    public ICollection<DocumentTag> DocumentTags { get; set; } = [];
    public ICollection<DocumentFile> DocumentFiles { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Report> Reports { get; set; } = [];
    public ICollection<DocumentReview> Reviews { get; set; } = [];

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

    public Guid? ReviewedById { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? ReviewNote { get; set; } = default!;
    public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.Approved;
}
