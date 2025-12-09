using MediatR;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Events;

public class TrustLevelChangedEvent : INotification
{
    public Guid UserId { get; set; }
    public TrustLever OldLevel { get; set; }
    public TrustLever NewLevel { get; set; }
    public int NewScore { get; set; }
    public int ScoreDelta { get; set; }
}
