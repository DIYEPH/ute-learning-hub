using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Author : SoftDeletableEntity<Guid>, IAggregateRoot, IAuditable
{
    public string FullName { get; set; } = default!;
    public string Description { get; set; } = default!;

    public ICollection<DocumentAuthor> DocumentAuthors { get; set; } = [];

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

    public ContentStatus Status { get; set; } = ContentStatus.Approved;
}



