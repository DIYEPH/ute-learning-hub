using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities.Base;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Notification.Commands.DeleteNotification;

public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteNotificationCommandHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to delete notifications");

        // Only admin can delete notifications
        var isAdmin = _currentUserService.IsInRole("Admin");
        if (!isAdmin)
            throw new UnauthorizedException("Only administrators can delete notifications");

        var userId = _currentUserService.UserId;
        var now = _dateTimeProvider.OffsetNow;

        // Get notification
        var notification = await _notificationRepository.GetByIdAsync(request.Id, cancellationToken: cancellationToken);
        if (notification == null)
            throw new NotFoundException($"Notification with id {request.Id} not found");

        // Soft delete notification
        await _notificationRepository.DeleteAsync(notification, userId, cancellationToken);

        // Soft delete all notification recipients using SoftDeleteExtensions
        var recipients = await _notificationRepository.GetNotificationRecipientsQueryable()
            .Where(nr => nr.NotificationId == request.Id && !nr.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var recipient in recipients)
        {
            recipient.MarkAsDeleted(userId, now);
        }

        await _notificationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
