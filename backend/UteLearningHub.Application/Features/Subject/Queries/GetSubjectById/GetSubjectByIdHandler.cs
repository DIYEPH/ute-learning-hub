using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Subject;

namespace UteLearningHub.Application.Features.Subject.Queries.GetSubjectById;

public class GetSubjectByIdHandler(ISubjectService subjectService, ICurrentUserService currentUserService) : IRequestHandler<GetSubjectByIdQuery, SubjectDetailDto>
{
    private readonly ISubjectService _subjectService = subjectService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<SubjectDetailDto> Handle(GetSubjectByIdQuery request, CancellationToken ct)
    {
        var isAdmin = _currentUserService.IsInRole("Admin");
        return await _subjectService.GetSubjectByIdAsync(request.Id, isAdmin, ct);
    }
}
