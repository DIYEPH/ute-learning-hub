using MediatR;
using Microsoft.Extensions.Logging;
using UteLearningHub.Application.Events;
using UteLearningHub.Application.Services.Recommendation;

namespace UteLearningHub.Application.EventHandlers;

public class UpdateVectorOnUserActivityHandler(IVectorMaintenanceService vectorMaintenanceService, ILogger<UpdateVectorOnUserActivityHandler> logger) : INotificationHandler<UserActivityEvent>
{
    private readonly IVectorMaintenanceService _vectorMaintenanceService = vectorMaintenanceService;
    private readonly ILogger<UpdateVectorOnUserActivityHandler> _logger = logger;

    public async Task Handle(UserActivityEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating user vector: {UserId}, Activity: {ActivityType}", notification.UserId, notification.ActivityType);

            await _vectorMaintenanceService.UpdateUserVectorAsync(notification.UserId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update vector: {UserId}, Activity: {ActivityType}", notification.UserId, notification.ActivityType);
        }
    }
}
