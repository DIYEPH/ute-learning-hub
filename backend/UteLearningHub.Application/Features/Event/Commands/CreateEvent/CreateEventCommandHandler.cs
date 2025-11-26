using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using DomainEvent = UteLearningHub.Domain.Entities.Event;

namespace UteLearningHub.Application.Features.Event.Commands.CreateEvent;

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, EventDto>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateEventCommandHandler(
        IEventRepository eventRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _eventRepository = eventRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<EventDto> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.IsInRole("Admin"))
            throw new UnauthorizedException("Only administrators can create events");

        if (request.StartAt >= request.EndAt)
            throw new BadRequestException("StartAt must be earlier than EndAt");

        var evt = new DomainEvent
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            ShortDescription = request.ShortDescription,
            Content = request.Content,
            ImageUrl = request.ImageUrl,
            RedirectUrl = request.RedirectUrl,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            IsActive = request.IsActive,
            Priority = request.Priority,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        await _eventRepository.AddAsync(evt, cancellationToken);
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