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

        var isAdmin = _currentUserService.IsInRole("Admin");

        IQueryable<NotificationDto> projectedQuery;

        // Admin uses admin query only when viewing from admin panel (IsDeleted filter provided)
        // For notification menu (personal view), admin uses same query as regular users
        if (isAdmin && request.IsDeleted.HasValue)
        {
            // Admin viewing from admin panel: query all notifications
            projectedQuery = GetAdminNotificationsQuery(request);
        }
        else
        {
            // Regular user OR admin viewing personal notifications: query via NotificationRecipient
            projectedQuery = GetUserNotificationsQuery(request);
        }

        // Apply common filters
        if (request.IsRead.HasValue)
        {
            projectedQuery = projectedQuery.Where(n => n.IsRead == request.IsRead.Value);
        }

        if (request.NotificationType.HasValue)
        {
            projectedQuery = projectedQuery.Where(n => n.NotificationType == request.NotificationType.Value);
        }

        if (request.NotificationPriorityType.HasValue)
        {
            projectedQuery = projectedQuery.Where(n => n.NotificationPriorityType == request.NotificationPriorityType.Value);
        }

        // Order by priority and created date
        projectedQuery = projectedQuery.OrderByDescending(n => n.NotificationPriorityType)
            .ThenByDescending(n => n.CreatedAt);

        var totalCount = await projectedQuery.CountAsync(cancellationToken);

        // Apply pagination
        var notifications = await projectedQuery
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

    private IQueryable<NotificationDto> GetAdminNotificationsQuery(GetNotificationsQuery request)
    {
        var query = _notificationRepository.GetQueryableSet().AsNoTracking();

        // IsDeleted filter for admin
        if (request.IsDeleted.HasValue)
        {
            query = query.Where(n => n.IsDeleted == request.IsDeleted.Value);
        }
        // If IsDeleted is null, show ALL items (both deleted and active) for admin

        return query.Select(n => new NotificationDto
        {
            Id = n.Id,
            ObjectId = n.ObjectId,
            Title = n.Title,
            Content = n.Content,
            Link = n.Link,
            IsGlobal = n.IsGlobal,
            ExpiredAt = n.ExpiredAt,
            NotificationType = n.NotificationType,
            NotificationPriorityType = n.NotificationPriorityType,
            IsRead = false,
            ReadAt = null,
            CreatedAt = n.CreatedAt,
            ReceivedAt = n.CreatedAt
        });
    }

    private IQueryable<NotificationDto> GetUserNotificationsQuery(GetNotificationsQuery request)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var query = _notificationRepository.GetNotificationRecipientsWithNotificationQueryable()
            .Where(nr => nr.RecipientId == userId
                && nr.Notification.ExpiredAt > DateTimeOffset.UtcNow
                && !nr.IsDeleted
                && !nr.Notification.IsDeleted)
            .AsNoTracking();

        return query.Select(nr => new NotificationDto
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
        });
    }
}
