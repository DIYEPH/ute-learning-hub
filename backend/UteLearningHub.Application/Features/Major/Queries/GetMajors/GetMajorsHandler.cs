using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Major.Queries.GetMajors;

public class GetMajorsHandler : IRequestHandler<GetMajorsQuery, PagedResponse<MajorDto>>
{
    private readonly IMajorRepository _majorRepository;

    public GetMajorsHandler(IMajorRepository majorRepository)
    {
        _majorRepository = majorRepository;
    }

    public async Task<PagedResponse<MajorDto>> Handle(GetMajorsQuery request, CancellationToken cancellationToken)
    {
        var query = _majorRepository.GetQueryableSet()
            .Include(m => m.Faculty)
            .AsNoTracking();

        // Filter by IsDeleted status (default: only active items)
        if (request.IsDeleted.HasValue)
            query = query.Where(m => m.IsDeleted == request.IsDeleted.Value);
        else
            query = query.Where(m => !m.IsDeleted);

        // Filter by FacultyId
        if (request.FacultyId.HasValue)
        {
            query = query.Where(m => m.FacultyId == request.FacultyId.Value);
        }

        // Search by name or code
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(m =>
                m.MajorName.ToLower().Contains(searchTerm) ||
                m.MajorCode.ToLower().Contains(searchTerm));
        }

        // Order by name
        query = query.OrderBy(m => m.MajorName);

        var totalCount = await query.CountAsync(cancellationToken);

        var majors = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(m => new MajorDto
            {
                Id = m.Id,
                MajorName = m.MajorName,
                MajorCode = m.MajorCode,
                Faculty = m.Faculty != null ? new FacultyDto
                {
                    Id = m.Faculty.Id,
                    FacultyName = m.Faculty.FacultyName,
                    FacultyCode = m.Faculty.FacultyCode,
                    Logo = m.Faculty.Logo
                } : null
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<MajorDto>
        {
            Items = majors,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
