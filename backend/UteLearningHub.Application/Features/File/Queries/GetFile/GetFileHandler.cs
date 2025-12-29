using MediatR;
using UteLearningHub.Application.Common.Results;
using UteLearningHub.Application.Common.Sercurity;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Application.Services.Identity;

namespace UteLearningHub.Application.Features.File.Queries.GetFile;

public class GetFileHandler : IRequestHandler<GetFileByIdQuery, FileStreamResult>
{
    private readonly IFileAccessService _fileAccessService;
    private readonly ICurrentUserService _currentUserService;

    public GetFileHandler(
        IFileAccessService fileAccessService,
        ICurrentUserService currentUserService)
    {
        _fileAccessService = fileAccessService;
        _currentUserService = currentUserService;
    }

    public async Task<FileStreamResult> Handle(GetFileByIdQuery request, CancellationToken ct)
    {
        var userContext = new UserContext(
        _currentUserService.UserId,
        _currentUserService.IsAuthenticated,
        _currentUserService.IsInRole("Admin"));

        return await _fileAccessService.GetFileByIdAsync(request.FileId, request.FileRequestType, userContext, ct);
    }
}

