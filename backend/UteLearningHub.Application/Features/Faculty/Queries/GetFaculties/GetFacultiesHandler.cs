using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Faculty;
using UteLearningHub.Application.Services.Identity;

namespace UteLearningHub.Application.Features.Faculty.Queries.GetFaculties;

public class GetFacultiesHandler(ICurrentUserService currentUserService, IFacultyService facultyService) : IRequestHandler<GetFacultiesQuery, PagedResponse<FacultyDetailDto>>
{
    private readonly IFacultyService _facultyService = facultyService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<PagedResponse<FacultyDetailDto>> Handle(GetFacultiesQuery request, CancellationToken ct)
    {
        var isAdmin = _currentUserService.IsInRole("Admin");
        return await _facultyService.GetFacultiesAsync(request, isAdmin, ct);
    }
}