using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Event.Commands.UpdateEvent;

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, EventDto>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateEventCommandHandler(
        IEventRepository eventRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<EventDto> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.IsInRole("Admin"))
            throw new UnauthorizedException("Only administrators can update events");

        if (request.StartAt >= request.EndAt)
            throw new BadRequestException("StartAt must be earlier than EndAt");

        var evt = await _eventRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);
        if (evt == null || evt.IsDeleted)
            throw new NotFoundException($"Event with id {request.Id} not found");

        evt.Title = request.Title;
        evt.ShortDescription = request.ShortDescription;
        evt.Content = request.Content;
        evt.ImageUrl = request.ImageUrl;
        evt.RedirectUrl = request.RedirectUrl;
        evt.StartAt = request.StartAt;
        evt.EndAt = request.EndAt;
        evt.IsActive = request.IsActive;
        evt.Priority = request.Priority;
        evt.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _eventRepository.UpdateAsync(evt, cancellationToken);
        await _eventRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return new EventDto
        {
            Id = evt.Id,
            Title = evt.Title,
            ShortDescription = evt.ShortDescription,
            Content = evt.Content,
            ImageUrl = evt.ImageUrl,
            RedirectUrl = evt.RedirectUrl,
            StartAt = evt.StartAt,
            EndAt = evt.EndAt,
            IsActive = evt.IsActive,
            Priority = evt.Priority
        };
    }
}