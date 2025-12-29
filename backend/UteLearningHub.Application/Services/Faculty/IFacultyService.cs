using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Faculty.Commands.CreateFaculty;
using UteLearningHub.Application.Features.Faculty.Commands.UpdateFaculty;
using UteLearningHub.Application.Features.Faculty.Queries.GetFaculties;

namespace UteLearningHub.Application.Services.Faculty;

public interface IFacultyService
{
    Task<FacultyDetailDto> GetFacultyByIdAsync(Guid id, bool isAdmin, CancellationToken ct);
    Task<PagedResponse<FacultyDetailDto>> GetFacultiesAsync(GetFacultiesQuery request, bool isAdmin, CancellationToken ct);
    Task<FacultyDetailDto> CreateAsync(Guid creatorId, CreateFacultyCommand request, CancellationToken ct);
    Task<FacultyDetailDto> UpdateAsync(Guid creatorId, UpdateFacultyCommand request, CancellationToken ct);
    Task SoftDeleteAsync(Guid facultyId, Guid actorId, CancellationToken ct);
}
