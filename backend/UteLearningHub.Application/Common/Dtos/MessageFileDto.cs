namespace UteLearningHub.Application.Common.Dtos;

public record MessageFileDto
{
    public Guid FileId { get; init; }
    public long FileSize { get; init; }
    public string MimeType { get; init; } = default!;
}
