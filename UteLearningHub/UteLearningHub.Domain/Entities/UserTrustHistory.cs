using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class UserTrustHistory : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public double Score { get; set; }
    public string Reason { get; set; } = default!;
}
