using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Domain.Policies;

public static class TrustLevelPolicy
{
    public static TrustLever Calculate(int trustScore)
    {
        return trustScore switch
        {
            < 5 => TrustLever.None,
            < 9 => TrustLever.Newbie,
            < 29 => TrustLever.Contributor,
            < 59 => TrustLever.TrustedMember,
            _ => TrustLever.Moderator
        };
    }
}

