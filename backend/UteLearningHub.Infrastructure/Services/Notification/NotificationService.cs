using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Notification.Commands.CreateNotification;
using UteLearningHub.Application.Features.Notification.Commands.DeleteNotification;
using UteLearningHub.Application.Features.Notification.Commands.MarkAllAsRead;
using UteLearningHub.Application.Features.Notification.Commands.MarkAsRead;
using UteLearningHub.Application.Features.Notification.Commands.UpdateNotification;
using UteLearningHub.Application.Services.Email;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Notification;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using NotificationEntity = UteLearningHub.Domain.Entities.Notification;

namespace UteLearningHub.Infrastructure.Services.Notification;

public class NotificationService(
    INotificationRepository notificationRepository,
    IUserService userService,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IEmailService emailService,
    IIdentityService identityService) : INotificationService
{
    public async Task<NotificationDto> CreateAsync(CreateNotificationCommand request, CancellationToken ct = default)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create notifications");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        // Only admin can create notifications
        var isAdmin = currentUserService.IsInRole("Admin");
        if (!isAdmin)
            throw new UnauthorizedException("Only administrators can create notifications");

        // Validate: if not global, must have recipients
        if (!request.IsGlobal && (request.RecipientIds == null || !request.RecipientIds.Any()))
            throw new BadRequestException("RecipientIds must be provided when IsGlobal is false");

        // Create notification
        var notification = new NotificationEntity
        {
            Id = Guid.NewGuid(),
            ObjectId = request.ObjectId,
            Title = request.Title,
            Content = request.Content,
            Link = request.Link,
            IsGlobal = request.IsGlobal,
            ExpiredAt = request.ExpiredAt,
            NotificationType = request.NotificationType,
            NotificationPriorityType = request.NotificationPriorityType,
            CreatedById = userId,
            CreatedAt = dateTimeProvider.OffsetNow
        };

        notificationRepository.Add(notification);

        // Create recipients
        IList<Guid> recipientIds;
        if (request.IsGlobal)
        {
            recipientIds = await userService.GetAllActiveUserIdsAsync(ct);
        }
        else
        {
            recipientIds = await userService.ValidateUserIdsAsync(request.RecipientIds!, ct);

            if (recipientIds.Count != request.RecipientIds!.Count)
                throw new BadRequestException("Some recipient IDs are invalid");
        }

        // Create notification recipients
        await notificationRepository.CreateNotificationRecipientsAsync(
            notification.Id,
            recipientIds,
            dateTimeProvider.OffsetNow,
            ct);

        await notificationRepository.UnitOfWork.SaveChangesAsync(ct);

        // Send email notification for high priority or notifications with links (async)
        if (request.NotificationPriorityType == NotificationPriorityType.Hight || !string.IsNullOrWhiteSpace(request.Link))
        {
            _ = SendEmailNotificationsAsync(request, recipientIds, ct);
        }

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
            ReceivedAt = dateTimeProvider.OffsetNow
        };
    }

    public async Task<NotificationDto> UpdateAsync(UpdateNotificationCommand request, CancellationToken ct = default)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update notifications");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        // Only admin can update notifications
        var isAdmin = currentUserService.IsInRole("Admin");
        if (!isAdmin)
            throw new UnauthorizedException("Only administrators can update notifications");

        var notification = await notificationRepository.GetByIdAsync(request.Id, cancellationToken: ct);
        if (notification == null)
            throw new NotFoundException($"Notification with id {request.Id} not found");

        notification.Title = request.Title;
        notification.Content = request.Content;
        notification.Link = request.Link ?? string.Empty;
        notification.ExpiredAt = request.ExpiredAt;
        notification.NotificationType = request.NotificationType;
        notification.NotificationPriorityType = request.NotificationPriorityType;
        notification.UpdatedById = userId;
        notification.UpdatedAt = dateTimeProvider.OffsetNow;

        notificationRepository.Update(notification);
        await notificationRepository.UnitOfWork.SaveChangesAsync(ct);

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

    public async Task DeleteAsync(DeleteNotificationCommand request, CancellationToken ct = default)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to delete notifications");

        var isAdmin = currentUserService.IsInRole("Admin");
        if (!isAdmin)
            throw new UnauthorizedException("Only administrators can delete notifications");

        var userId = currentUserService.UserId;
        var now = dateTimeProvider.OffsetNow;

        var notification = await notificationRepository.GetByIdAsync(request.Id, cancellationToken: ct);
        if (notification == null)
            throw new NotFoundException($"Notification with id {request.Id} not found");

        notification.IsDeleted = true;
        notification.DeletedAt = dateTimeProvider.OffsetUtcNow;
        notification.DeletedById = userId;

        notificationRepository.Update(notification);

        var recipients = await notificationRepository.GetNotificationRecipientsQueryable()
            .Where(nr => nr.NotificationId == request.Id && !nr.IsDeleted)
            .ToListAsync(ct);

        foreach (var recipient in recipients)
        {
            recipient.DeletedById = userId;
            recipient.DeletedAt = dateTimeProvider.OffsetUtcNow;
            recipient.IsDeleted = true;
            notificationRepository.UpdateRecipient(recipient);
        }

        await notificationRepository.UnitOfWork.SaveChangesAsync(ct);
    }

    public async Task MarkAsReadAsync(MarkAsReadCommand request, CancellationToken ct = default)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to mark notifications as read");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        var recipient = await notificationRepository.GetNotificationRecipientAsync(
            request.NotificationId,
            userId,
            disableTracking: false,
            ct);

        if (recipient == null)
            throw new NotFoundException($"Notification with id {request.NotificationId} not found");

        if (!recipient.IsRead)
        {
            recipient.IsRead = true;
            recipient.ReadAt = dateTimeProvider.OffsetNow;
            await notificationRepository.UnitOfWork.SaveChangesAsync(ct);
        }
    }

    public async Task MarkAllAsReadAsync(MarkAllAsReadCommand request, CancellationToken ct = default)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to mark notifications as read");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        var unreadRecipients = await notificationRepository.GetUnreadNotificationRecipientsAsync(userId, ct);

        var now = dateTimeProvider.OffsetNow;
        foreach (var recipient in unreadRecipients)
        {
            recipient.IsRead = true;
            recipient.ReadAt = now;
        }

        if (unreadRecipients.Any())
        {
            await notificationRepository.UnitOfWork.SaveChangesAsync(ct);
        }
    }

    private async Task SendEmailNotificationsAsync(CreateNotificationCommand request, IList<Guid> recipientIds, CancellationToken ct)
    {
        try
        {
            var recipientEmails = new List<string>();

            foreach (var recipientId in recipientIds)
            {

                var user = await identityService.FindByIdAsync(recipientId);
                if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                    recipientEmails.Add(user.Email);
            }

            if (recipientEmails.Any())
            {
                var notificationLink = !string.IsNullOrWhiteSpace(request.Link) ? request.Link : null ;

                await emailService.SendEmailAsync(
                    recipientEmails,
                    $"Thông báo: {request.Title}",
                    $"<h2>{request.Title}</h2><p>{request.Content}</p>" +
                    (!string.IsNullOrWhiteSpace(notificationLink)
                        ? $"<p><a href='{notificationLink}'>Xem chi tiết</a></p>"
                        : ""),
                    isHtml: true,
                    ct);
            }
        }
        catch { }
    }
}
