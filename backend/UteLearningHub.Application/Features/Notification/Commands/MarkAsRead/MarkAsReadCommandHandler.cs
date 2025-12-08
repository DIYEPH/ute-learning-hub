using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Notification.Commands.MarkAsRead;

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, Unit>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public MarkAsReadCommandHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to mark notifications as read");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var recipient = await _notificationRepository.GetNotificationRecipientAsync(
            request.NotificationId,
            userId,
            disableTracking: false,
            cancellationToken);

        if (recipient == null)
            throw new NotFoundException($"Notification with id {request.NotificationId} not found");

        if (!recipient.IsRead)
        {
            recipient.IsRead = true;
            recipient.ReadAt = _dateTimeProvider.OffsetNow;
            await _notificationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}