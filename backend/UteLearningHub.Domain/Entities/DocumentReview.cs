using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class DocumentReview : BaseEntity<Guid>, IAuditable, IAggregateRoot
{
    public Guid DocumentId { get; set; }
    public Guid DocumentFileId { get; set; }
    public DocumentReviewType DocumentReviewType { get; set; }
    public Document Document { get; set; } = default!;
    public DocumentFile DocumentFile { get; set; } = default!;

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
}
