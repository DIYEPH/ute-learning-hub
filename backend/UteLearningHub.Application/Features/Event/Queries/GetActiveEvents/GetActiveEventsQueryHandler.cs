using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Event.Queries.GetActiveEvents;

public class GetActiveEventsHandler 
    : IRequestHandler<GetActiveEventsQuery, IReadOnlyList<EventDto>>
{
    private readonly IEventRepository _eventRepository;

    public GetActiveEventsHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<IReadOnlyList<EventDto>> Handle(
        GetActiveEventsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _eventRepository.GetActiveEvents(request.Take);

        var events = await query
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return events.Select(e => new EventDto
        {
            Id = e.Id,
            Title = e.Title,
            ShortDescription = e.ShortDescription,
            Content = e.Content,
            ImageUrl = e.ImageUrl,
            RedirectUrl = e.RedirectUrl,
            StartAt = e.StartAt,
            EndAt = e.EndAt,
            IsActive = e.IsActive,
            Priority = e.Priority
        }).ToList();
    }
}