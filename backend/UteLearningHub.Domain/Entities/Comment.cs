using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Comment : SoftDeletableEntity<Guid>, IAuditable, IReviewable, IAggregateRoot
{
    public Guid DocumentFileId { get; set; }
    public Guid? ParentId { get; set; }
    public string Content { get; set; } = default!;
    public DocumentFile DocumentFile { get; set; } = default!;
    public ICollection<Comment> Childrens { get; set; } = [];
    public ICollection<Report> Reports { get; set; } = [];

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

    public Guid? ReviewedById { get; set; }
    public string? ReviewNote { get; set; } = default!;
    public DateTimeOffset? ReviewedAt { get; set; }
    public ReviewStatus ReviewStatus { get; set; }
}
