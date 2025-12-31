namespace UteLearningHub.Domain.Policies;

public static class ProposalSettings
{
    // === Proposal mới ===
    public const int MinMembersToActivate = 3;
    public const int ProposalExpirationDays = 7;

    // === Giới hạn user ===
    public const int MaxActiveConversations = 10;
    public const int MaxPendingProposals = 1;
    public const int CooldownDaysAfterDecline = 1;

    // === Gợi ý nhóm có sẵn ===
    public const float MinSimilarityForExistingGroup = 0.7f;
    public const int MaxExistingGroupSuggestions = 3;
    public const int SuggestionCooldownDays = 3;
}

