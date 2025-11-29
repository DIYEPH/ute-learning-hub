using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Major.Commands.DeleteMajor;

public class DeleteMajorCommandHandler : IRequestHandler<DeleteMajorCommand, Unit>
{
    private readonly IMajorRepository _majorRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteMajorCommandHandler(
        IMajorRepository majorRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _majorRepository = majorRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(DeleteMajorCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to delete majors");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var major = await _majorRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);

        if (major == null || major.IsDeleted)
            throw new NotFoundException($"Major with id {request.Id} not found");

        // Check if major is being used by any subjects
        var subjectCount = await _majorRepository.GetQueryableSet()
            .Where(m => m.Id == request.Id)
            .Select(m => m.SubjectMajors.Count)
            .FirstOrDefaultAsync(cancellationToken);

        if (subjectCount > 0)
            throw new BadRequestException($"Cannot delete major. It is being used by {subjectCount} subject(s)");

        // Hard delete
        await _majorRepository.DeleteAsync(major, cancellationToken);
        await _majorRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}