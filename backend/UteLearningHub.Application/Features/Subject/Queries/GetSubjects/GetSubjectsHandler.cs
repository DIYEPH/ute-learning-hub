using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Subject;

namespace UteLearningHub.Application.Features.Subject.Queries.GetSubjects;

public class GetSubjectsHandler(ISubjectService subjectService, ICurrentUserService currentUserService) : IRequestHandler<GetSubjectsQuery, PagedResponse<SubjectDetailDto>>
{
    private readonly ISubjectService _subjectService = subjectService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<PagedResponse<SubjectDetailDto>> Handle(GetSubjectsQuery request, CancellationToken ct)
    {
        var isAdmin = _currentUserService.IsInRole("Admin");
        return await _subjectService.GetSubjectsAsync(request, isAdmin, ct);
    }
}
