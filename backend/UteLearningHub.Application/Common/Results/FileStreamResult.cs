namespace UteLearningHub.Application.Common.Results;

public record FileStreamResult(
    Stream Stream,
    string MimeType
);