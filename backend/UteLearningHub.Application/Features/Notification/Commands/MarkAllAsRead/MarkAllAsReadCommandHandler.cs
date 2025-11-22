using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Notification.Commands.MarkAllAsRead;

public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, Unit>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public MarkAllAsReadCommandHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to mark notifications as read");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var unreadRecipients = await _notificationRepository.GetUnreadNotificationRecipientsAsync(userId, cancellationToken);

        var now = _dateTimeProvider.OffsetNow;
        foreach (var recipient in unreadRecipients)
        {
            recipient.IsRead = true;
            recipient.ReadAt = now;
        }

        if (unreadRecipients.Any())
        {
            await _notificationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}