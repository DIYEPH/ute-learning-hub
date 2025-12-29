using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Subject;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Subject.Commands.CreateSubject;

public class CreateSubjectCommandHandler(ISubjectService subjectService, ICurrentUserService currentUserService) : IRequestHandler<CreateSubjectCommand, SubjectDetailDto>
{
    private readonly ISubjectService _subjectService = subjectService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<SubjectDetailDto> Handle(CreateSubjectCommand request, CancellationToken ct)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenException("Only admin can create subject");

        var actorId = _currentUserService.UserId!.Value;

        return await _subjectService.CreateAsync(actorId, request, ct);

    }
}
