using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Event.Commands.DeleteEvent;

public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, Unit>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteEventCommandHandler(
        IEventRepository eventRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.IsInRole("Admin"))
            throw new UnauthorizedException("Only administrators can delete events");

        var evt = await _eventRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);
        if (evt == null || evt.IsDeleted)
            throw new NotFoundException($"Event with id {request.Id} not found");

        evt.IsDeleted = true;
        evt.DeletedAt = _dateTimeProvider.OffsetNow;
        evt.DeletedById = _currentUserService.UserId;

        await _eventRepository.UpdateAsync(evt, cancellationToken);
        await _eventRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}