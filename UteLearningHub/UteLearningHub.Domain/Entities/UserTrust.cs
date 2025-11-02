using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Domain.Entities;

public class UserTrust 
{
    public Guid UserId { get; set; }
    public int TrustScrore { get; set; } = 0;
    public TrustLever TrustLever { get; set; } = TrustLever.None;
}
