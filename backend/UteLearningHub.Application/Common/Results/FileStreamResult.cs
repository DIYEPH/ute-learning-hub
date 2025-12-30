namespace UteLearningHub.Application.Common.Results;

public record FileStreamResult(Stream? Stream, string MimeType, string? RedirectUrl = null)
{
    public bool IsRedirect => !string.IsNullOrEmpty(RedirectUrl);
}
