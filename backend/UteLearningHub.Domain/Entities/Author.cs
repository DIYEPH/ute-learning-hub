using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Author : SoftDeletableEntity<Guid>, IAggregateRoot, IAuditable, IReviewable
{
    public string FullName { get; set; } = default!;
    public string Description { get; set; } = default!;

    public ICollection<DocumentAuthor> DocumentAuthors { get; set; } = [];

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

    public Guid? ReviewedById { get; set; }
    public string? ReviewNote { get; set; } = default!;
    public DateTimeOffset? ReviewedAt { get; set; }
    public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.Approved;
}


