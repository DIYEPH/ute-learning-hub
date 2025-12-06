namespace UteLearningHub.Application.Features.File.Queries.GetFile;

public class GetFileResponse
{
    public Stream FileStream { get; init; } = default!;
    public string MimeType { get; init; } = default!;
}

