using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Subject;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Subject.Commands.DeleteSubject;

public class DeleteSubjectCommandHandler(ISubjectService subjectService, ICurrentUserService currentUserService) : IRequestHandler<DeleteSubjectCommand>
{
    private readonly ISubjectService _subjectService = subjectService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task Handle(DeleteSubjectCommand request, CancellationToken ct)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenException("Only admin can delete subjects");

        var actorId = _currentUserService.UserId!.Value;

        await _subjectService.SoftDeleteAsync(request.Id, actorId, ct);
    }
}