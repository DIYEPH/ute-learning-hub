using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Notification.Commands.UpdateNotification;

public class UpdateNotificationCommandHandler : IRequestHandler<UpdateNotificationCommand, NotificationDto>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateNotificationCommandHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<NotificationDto> Handle(UpdateNotificationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update notifications");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Only admin can update notifications
        var isAdmin = _currentUserService.IsInRole("Admin");
        if (!isAdmin)
            throw new UnauthorizedException("Only administrators can update notifications");

        // Get notification
        var notification = await _notificationRepository.GetByIdAsync(request.Id, cancellationToken: cancellationToken);
        if (notification == null)
            throw new NotFoundException($"Notification with id {request.Id} not found");

        // Update fields
        notification.Title = request.Title;
        notification.Content = request.Content;
        notification.Link = request.Link ?? string.Empty;
        notification.ExpiredAt = request.ExpiredAt;
        notification.NotificationType = request.NotificationType;
        notification.NotificationPriorityType = request.NotificationPriorityType;
        notification.UpdatedById = userId;
        notification.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _notificationRepository.UpdateAsync(notification, cancellationToken);

        return new NotificationDto
        {
            Id = notification.Id,
            ObjectId = notification.ObjectId,
            Title = notification.Title,
            Content = notification.Content,
            Link = notification.Link,
            IsGlobal = notification.IsGlobal,
            ExpiredAt = notification.ExpiredAt,
            NotificationType = notification.NotificationType,
            NotificationPriorityType = notification.NotificationPriorityType,
            IsRead = false,
            ReadAt = null,
            CreatedAt = notification.CreatedAt,
            ReceivedAt = null
        };
    }
}
