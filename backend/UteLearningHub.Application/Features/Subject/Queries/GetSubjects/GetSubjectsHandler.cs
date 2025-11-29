using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Subject.Queries.GetSubjects;

public class GetSubjectsHandler : IRequestHandler<GetSubjectsQuery, PagedResponse<SubjectDto>>
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetSubjectsHandler(ISubjectRepository subjectRepository, ICurrentUserService currentUserService)
    {
        _subjectRepository = subjectRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResponse<SubjectDto>> Handle(GetSubjectsQuery request, CancellationToken cancellationToken)
    {
        var query = _subjectRepository.GetQueryableSet()
            .Include(s => s.SubjectMajors)
                .ThenInclude(sm => sm.Major)
                    .ThenInclude(m => m.Faculty)
            .AsNoTracking();

        // Filter by MajorIds
        if (request.MajorIds?.Any() == true)
        {
            query = query.Where(s => s.SubjectMajors.Any(sm => request.MajorIds.Contains(sm.MajorId)));
        }

        // Search by name or code
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(s =>
                s.SubjectName.ToLower().Contains(searchTerm) ||
                s.SubjectCode.ToLower().Contains(searchTerm));
        }

        var isAdmin = _currentUserService.IsAuthenticated && _currentUserService.IsInRole("Admin");
        if (!isAdmin)
        {
            query = query.Where(s => s.ReviewStatus == ReviewStatus.Approved);
        }

        // Order by name
        query = query.OrderBy(s => s.SubjectName);

        var totalCount = await query.CountAsync(cancellationToken);

        var subjects = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(s => new SubjectDto
            {
                Id = s.Id,
                SubjectName = s.SubjectName,
                SubjectCode = s.SubjectCode,
                Majors = s.SubjectMajors.Select(sm => new MajorDto
                {
                    Id = sm.Major.Id,
                    MajorName = sm.Major.MajorName,
                    MajorCode = sm.Major.MajorCode,
                    Faculty = sm.Major.Faculty != null ? new FacultyDto
                    {
                        Id = sm.Major.Faculty.Id,
                        FacultyName = sm.Major.Faculty.FacultyName,
                        FacultyCode = sm.Major.Faculty.FacultyCode
                    } : null
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<SubjectDto>
        {
            Items = subjects,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
