using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Major;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Major.Commands.DeleteMajor;

public class DeleteMajorCommandHandler(IMajorService majorService, ICurrentUserService currentUserService) : IRequestHandler<DeleteMajorCommand>
{
    private readonly IMajorService _majorService = majorService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task Handle(DeleteMajorCommand request, CancellationToken ct)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenException("Only admin can delete majors");

        var actorId = _currentUserService.UserId!.Value;

        await _majorService.SoftDeleteAsync(request.Id, actorId, ct);
    }
}