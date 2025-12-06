namespace UteLearningHub.Application.Common.Dtos;

public class FileDto
{
    public Guid Id { get; init; }
    public long FileSize { get; init; }
    public string MimeType { get; init; } = default!;
}

