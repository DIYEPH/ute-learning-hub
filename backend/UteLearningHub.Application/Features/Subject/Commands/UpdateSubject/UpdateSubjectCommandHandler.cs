using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Subject;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Subject.Commands.UpdateSubject;

public class UpdateSubjectCommandHandler(ISubjectService subjectService, ICurrentUserService currentUserService) : IRequestHandler<UpdateSubjectCommand, SubjectDetailDto>
{
    private readonly ISubjectService _subjectService = subjectService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<SubjectDetailDto> Handle(UpdateSubjectCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenException("Only admin can update subjects");

        var actorId = _currentUserService.UserId!.Value;

        return await _subjectService.UpdateAsync(actorId, request, cancellationToken);
    }
}