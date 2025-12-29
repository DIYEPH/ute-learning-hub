using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Faculty;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Faculty.Commands.UpdateFaculty;

public class UpdateFacultyCommandHandler(ICurrentUserService currentUserService, IFacultyService facultyService) : IRequestHandler<UpdateFacultyCommand, FacultyDetailDto>
{
    private readonly IFacultyService _facultyService = facultyService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<FacultyDetailDto> Handle(UpdateFacultyCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenException("Only admin can delete faculties");

        var actorId = _currentUserService.UserId!.Value;

        return await _facultyService.UpdateAsync(actorId, request, cancellationToken);
    }
}

