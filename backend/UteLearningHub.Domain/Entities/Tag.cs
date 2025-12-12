using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Tag : SoftDeletableEntity<Guid>, IAggregateRoot, IAuditable
{
    public string TagName { get; set; } = default!;
    public ICollection<DocumentTag> DocumentTags { get; set; } = [];

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

    public ContentStatus Status { get; set; } = ContentStatus.Approved;
}

