using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Faculty;
using UteLearningHub.Application.Services.Identity;

namespace UteLearningHub.Application.Features.Faculty.Queries.GetFacultyById;

public class GetFacultyByIdHandler(ICurrentUserService currentUserService, IFacultyService facultyService) : IRequestHandler<GetFacultyByIdQuery, FacultyDetailDto>
{
    private readonly IFacultyService _facultyService = facultyService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<FacultyDetailDto> Handle(GetFacultyByIdQuery request, CancellationToken ct)
    {
        var isAdmin = _currentUserService.IsInRole("Admin");
        return await _facultyService.GetFacultyByIdAsync(request.Id, isAdmin, ct);
    }
}