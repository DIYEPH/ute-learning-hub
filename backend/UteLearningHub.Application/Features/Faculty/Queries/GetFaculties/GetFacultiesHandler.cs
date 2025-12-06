using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Faculty.Queries.GetFaculties;

public class GetFacultiesHandler : IRequestHandler<GetFacultiesQuery, PagedResponse<FacultyDto>>
{
    private readonly IFacultyRepository _facultyRepository;

    public GetFacultiesHandler(IFacultyRepository facultyRepository)
    {
        _facultyRepository = facultyRepository;
    }

    public async Task<PagedResponse<FacultyDto>> Handle(GetFacultiesQuery request, CancellationToken cancellationToken)
    {
        var query = _facultyRepository.GetQueryableSet()
            .AsNoTracking();

        // Filter by IsDeleted status (default: only active items)
        if (request.IsDeleted.HasValue)
            query = query.Where(f => f.IsDeleted == request.IsDeleted.Value);
        else
            query = query.Where(f => !f.IsDeleted);

        // Search by name or code
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(f =>
                f.FacultyName.ToLower().Contains(searchTerm) ||
                f.FacultyCode.ToLower().Contains(searchTerm));
        }

        // Order by name
        query = query.OrderBy(f => f.FacultyName);

        var totalCount = await query.CountAsync(cancellationToken);

        var faculties = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(f => new FacultyDto
            {
                Id = f.Id,
                FacultyName = f.FacultyName,
                FacultyCode = f.FacultyCode,
                Logo = f.Logo
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<FacultyDto>
        {
            Items = faculties,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}