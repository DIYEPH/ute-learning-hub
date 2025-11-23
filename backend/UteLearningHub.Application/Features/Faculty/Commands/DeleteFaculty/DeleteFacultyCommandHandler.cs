using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Faculty.Commands.DeleteFaculty;

public class DeleteFacultyCommandHandler : IRequestHandler<DeleteFacultyCommand, Unit>
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteFacultyCommandHandler(
        IFacultyRepository facultyRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _facultyRepository = facultyRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(DeleteFacultyCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to delete faculties");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var faculty = await _facultyRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);

        if (faculty == null || faculty.IsDeleted)
            throw new NotFoundException($"Faculty with id {request.Id} not found");

        // Check if faculty is being used by any majors
        var majorCount = await _facultyRepository.GetQueryableSet()
            .Where(f => f.Id == request.Id)
            .Select(f => f.Majors.Count(m => !m.IsDeleted))
            .FirstOrDefaultAsync(cancellationToken);

        if (majorCount > 0)
            throw new BadRequestException($"Cannot delete faculty. It is being used by {majorCount} major(s)");

        // Hard delete
        await _facultyRepository.DeleteAsync(faculty, cancellationToken);
        await _facultyRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
