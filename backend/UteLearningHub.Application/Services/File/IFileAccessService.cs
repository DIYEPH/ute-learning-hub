using UteLearningHub.Application.Common.Results;
using UteLearningHub.Application.Common.Sercurity;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Services.File;

public interface IFileAccessService
{
    Task<FileStreamResult> GetFileByIdAsync(Guid fileId, FileRequestType fileType, UserContext user, CancellationToken ct);
}
