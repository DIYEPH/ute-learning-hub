using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Common.Dtos;

public record ConversationMemberDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = default!;
    public string? UserAvatarUrl { get; init; }
    public ConversationMemberRoleType RoleType { get; init; }
    public bool IsMuted { get; init; }
    public DateTimeOffset JoinedAt { get; init; }
}
