using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class DocumentReview : BaseEntity<Guid>, IAuditable<Guid>
{
    public Guid DocumentId { get; set; }
    public DocumentReviewType DocumentReviewType { get; set; }
    public Document Document { get; set; } = default!;

    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }
}
