using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Faculty;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Faculty.Commands.CreateFaculty;

public class CreateFacultyCommandHandler(ICurrentUserService currentUserService, IFacultyService facultyService) : IRequestHandler<CreateFacultyCommand, FacultyDetailDto>
{
    private readonly IFacultyService _facultyService = facultyService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<FacultyDetailDto> Handle(CreateFacultyCommand request, CancellationToken ct)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenException("Only admin can create faculties");

        var actorId = _currentUserService.UserId!.Value;

        return await _facultyService.CreateAsync(actorId, request, ct);
    }
}
