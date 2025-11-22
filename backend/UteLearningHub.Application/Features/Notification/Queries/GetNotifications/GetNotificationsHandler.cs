using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Notification.Queries.GetNotifications;

public class GetNotificationsHandler : IRequestHandler<GetNotificationsQuery, PagedResponse<NotificationDto>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetNotificationsHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResponse<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to view notifications");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Query notifications for current user from NotificationRecipient with Notification included
        var query = _notificationRepository.GetNotificationRecipientsWithNotificationQueryable()
            .Where(nr => nr.RecipientId == userId 
                && !nr.IsDeleted 
                && !nr.Notification.IsDeleted
                && nr.Notification.ExpiredAt > DateTimeOffset.UtcNow)
            .Select(nr => new NotificationDto
            {
                Id = nr.Notification.Id,
                ObjectId = nr.Notification.ObjectId,
                Title = nr.Notification.Title,
                Content = nr.Notification.Content,
                Link = nr.Notification.Link,
                IsGlobal = nr.Notification.IsGlobal,
                ExpiredAt = nr.Notification.ExpiredAt,
                NotificationType = nr.Notification.NotificationType,
                NotificationPriorityType = nr.Notification.NotificationPriorityType,
                IsRead = nr.IsRead,
                ReadAt = nr.ReadAt,
                CreatedAt = nr.Notification.CreatedAt,
                ReceivedAt = nr.ReceivedAt
            })
            .AsNoTracking();

        // Filters
        if (request.IsRead.HasValue)
        {
            query = query.Where(n => n.IsRead == request.IsRead.Value);
        }

        if (request.NotificationType.HasValue)
        {
            query = query.Where(n => n.NotificationType == request.NotificationType.Value);
        }

        if (request.NotificationPriorityType.HasValue)
        {
            query = query.Where(n => n.NotificationPriorityType == request.NotificationPriorityType.Value);
        }

        // Order by priority and created date
        query = query.OrderByDescending(n => n.NotificationPriorityType)
            .ThenByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var notifications = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        return new PagedResponse<NotificationDto>
        {
            Items = notifications,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
