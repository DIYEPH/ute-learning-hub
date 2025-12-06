using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Subject.Commands.DeleteSubject;

public class DeleteSubjectCommandHandler : IRequestHandler<DeleteSubjectCommand, Unit>
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteSubjectCommandHandler(
        ISubjectRepository subjectRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _subjectRepository = subjectRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(DeleteSubjectCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to delete subjects");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var subject = await _subjectRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);

        if (subject == null || subject.IsDeleted)
            throw new NotFoundException($"Subject with id {request.Id} not found");

        // Check if subject is being used by any documents
        var documentCount = await _subjectRepository.GetQueryableSet()
            .Where(s => s.Id == request.Id)
            .Select(s => s.Documents.Count(d => !d.IsDeleted))
            .FirstOrDefaultAsync(cancellationToken);

        if (documentCount > 0)
            throw new BadRequestException($"Cannot delete subject. It is being used by {documentCount} document(s)");

        // Hard delete
        await _subjectRepository.DeleteAsync(subject, userId, cancellationToken);
        await _subjectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}