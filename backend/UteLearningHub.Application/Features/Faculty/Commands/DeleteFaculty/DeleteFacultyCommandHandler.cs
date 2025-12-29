using MediatR;
using UteLearningHub.Application.Services.Faculty;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Faculty.Commands.DeleteFaculty;

public class DeleteFacultyCommandHandler(ICurrentUserService currentUserService, IFacultyService facultyService) : IRequestHandler<DeleteFacultyCommand>
{
    private readonly IFacultyService _facultyService = facultyService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task Handle(DeleteFacultyCommand request, CancellationToken ct)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenException("Only admin can delete faculties");

        var actorId = _currentUserService.UserId!.Value;

        await _facultyService.SoftDeleteAsync(request.Id, actorId, ct);
    }
}
