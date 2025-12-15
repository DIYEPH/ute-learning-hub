using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class UserTrustHistory : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public double Score { get; set; }
    public double OldScore { get; set; }
    public double NewScore { get; set; }
    public string Reason { get; set; } = default!;
    public Guid? EntityId { get; set; }
    public TrustEntityType? EntityType { get; set; }
}
