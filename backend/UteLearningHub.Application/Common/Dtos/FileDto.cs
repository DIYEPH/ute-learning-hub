namespace UteLearningHub.Application.Common.Dtos;

public class FileDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = default!;
    public string FileUrl { get; init; } = default!;
    public long FileSize { get; init; }
    public string MimeType { get; init; } = default!;
    public bool IsTemporary { get; init; }
}


