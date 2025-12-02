using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Common.Dtos;

public record MessageDto
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public Guid? ParentId { get; init; }
    public string Content { get; init; } = default!;
    public bool IsEdit { get; init; }
    public bool IsPined { get; init; }
    public MessageType? Type { get; init; }
    public Guid CreatedById { get; init; }
    public string SenderName { get; init; } = default!;
    public string? SenderAvatarUrl { get; init; }
    public IEnumerable<MessageFileDto> Files { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public record MessageFileDto
{
    public Guid FileId { get; init; }
    public string FileName { get; init; } = default!;
    public string FileUrl { get; init; } = default!;
    public long FileSize { get; init; }
    public string MimeType { get; init; } = default!;
}