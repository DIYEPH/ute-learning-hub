using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Email;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using NotificationEntity = UteLearningHub.Domain.Entities.Notification;

namespace UteLearningHub.Application.Features.Notification.Commands.CreateNotification;

public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, NotificationDto>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IEmailService _emailService;
    private readonly IIdentityService _identityService;

    public CreateNotificationCommandHandler(
        INotificationRepository notificationRepository,
        IUserService userService,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IEmailService emailService,
        IIdentityService identityService)
    {
        _notificationRepository = notificationRepository;
        _userService = userService;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _emailService = emailService;
        _identityService = identityService;
    }

    public async Task<NotificationDto> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create notifications");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Only admin can create notifications
        var isAdmin = _currentUserService.IsInRole("Admin");
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
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);

        // Create recipients
        IList<Guid> recipientIds;
        if (request.IsGlobal)
        {
            // For global notifications, get all active users
            recipientIds = await _userService.GetAllActiveUserIdsAsync(cancellationToken);
        }
        else
        {
            // Validate recipient IDs exist
            recipientIds = await _userService.ValidateUserIdsAsync(request.RecipientIds!, cancellationToken);
            
            if (recipientIds.Count != request.RecipientIds!.Count)
                throw new BadRequestException("Some recipient IDs are invalid");
        }

        // Create notification recipients
        await _notificationRepository.CreateNotificationRecipientsAsync(
            notification.Id, 
            recipientIds, 
            _dateTimeProvider.OffsetNow, 
            cancellationToken);

        await _notificationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Gửi email notification cho recipients (async, không block response)
        // Chỉ gửi cho notification quan trọng hoặc khi có link
        if (request.NotificationPriorityType == NotificationPriorityType.Hight || 
            !string.IsNullOrWhiteSpace(request.Link))
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    // Lấy emails của recipients (giới hạn để tránh spam)
                    var recipientEmails = new List<string>();
                    var maxEmails = 100; // Giới hạn số lượng email để tránh spam
                    var count = 0;
                    
                    foreach (var recipientId in recipientIds)
                    {
                        if (count >= maxEmails) break;
                        
                        var user = await _identityService.FindByIdAsync(recipientId);
                        if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                        {
                            recipientEmails.Add(user.Email);
                            count++;
                        }
                    }

                    // Gửi email cho recipients
                    if (recipientEmails.Any())
                    {
                        var notificationLink = !string.IsNullOrWhiteSpace(request.Link) 
                            ? request.Link.StartsWith("http") 
                                ? request.Link 
                                : $"http://localhost:3000{request.Link}"
                            : null;

                        await _emailService.SendEmailAsync(
                            recipientEmails,
                            $"Thông báo: {request.Title}",
                            $"<h2>{request.Title}</h2><p>{request.Content}</p>" + 
                            (!string.IsNullOrWhiteSpace(notificationLink) 
                                ? $"<p><a href='{notificationLink}'>Xem chi tiết</a></p>" 
                                : ""),
                            isHtml: true,
                            cancellationToken);
                    }
                }
                catch
                {
                    // Log error nhưng không throw để không ảnh hưởng đến response
                }
            }, cancellationToken);
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
            ReceivedAt = _dateTimeProvider.OffsetNow
        };
    }
}
