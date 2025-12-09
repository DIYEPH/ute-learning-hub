using MediatR;
using Microsoft.Extensions.Logging;
using UteLearningHub.Application.Events;
using UteLearningHub.Application.Services.Recommendation;

namespace UteLearningHub.Application.EventHandlers;

public class UpdateVectorOnUserActivityHandler : INotificationHandler<UserActivityEvent>
{
    private readonly IVectorMaintenanceService _vectorMaintenanceService;
    private readonly ILogger<UpdateVectorOnUserActivityHandler> _logger;

    public UpdateVectorOnUserActivityHandler(
        IVectorMaintenanceService vectorMaintenanceService,
        ILogger<UpdateVectorOnUserActivityHandler> logger)
    {
        _vectorMaintenanceService = vectorMaintenanceService;
        _logger = logger;
    }

    public async Task Handle(UserActivityEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Updating user vector for UserId: {UserId}, Activity: {ActivityType}", 
                notification.UserId, 
                notification.ActivityType);

            await _vectorMaintenanceService.UpdateUserVectorAsync(notification.UserId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to update vector for UserId: {UserId}, Activity: {ActivityType}", 
                notification.UserId, 
                notification.ActivityType);
            // Don't throw - vector update is non-critical
        }
    }
}
