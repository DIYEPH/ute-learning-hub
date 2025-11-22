using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Subject.Queries.GetSubjects;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Subject.Queries.GetSubjects;

public class GetSubjectsHandler : IRequestHandler<GetSubjectsQuery, PagedResponse<SubjectDto>>
{
    private readonly ISubjectRepository _subjectRepository;

    public GetSubjectsHandler(ISubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    public async Task<PagedResponse<SubjectDto>> Handle(GetSubjectsQuery request, CancellationToken cancellationToken)
    {
        var query = _subjectRepository.GetQueryableSet()
            .Include(s => s.Major)
                .ThenInclude(m => m.Faculty)
            .AsNoTracking();

        // Filter by MajorId
        if (request.MajorId.HasValue)
        {
            query = query.Where(s => s.MajorId == request.MajorId.Value);
        }

        // Search by name or code
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(s =>
                s.SubjectName.ToLower().Contains(searchTerm) ||
                s.SubjectCode.ToLower().Contains(searchTerm));
        }

        // Only show approved subjects
        query = query.Where(s => s.ReviewStatus == ReviewStatus.Approved);

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
                SubjectCode = s.SubjectCode
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
