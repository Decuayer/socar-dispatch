using MediatR;
using Microsoft.Extensions.Logging;
using SocarDispatch.Domain.Events;

namespace SocarDispatch.Infrastructure.Notifications;

/// Notification handler that executes when an IncidentStatusChangedEvent is raised.
/// It logs the status change and provides the infrastructure for triggering SDDC-15 / SignalR live broadcasts (BroadcastIncidentStatusChanged).
public class IncidentStatusChangedNotificationHandler : INotificationHandler<IncidentStatusChangedEvent>
{
    private readonly ILogger<IncidentStatusChangedNotificationHandler> _logger;

    public IncidentStatusChangedNotificationHandler(ILogger<IncidentStatusChangedNotificationHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(IncidentStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Incident status changed notification received. IncidentId: {IncidentId}, PreviousStatus: {PreviousStatus}, NewStatus: {NewStatus}, ChangedById: {ChangedById}, ChangedAt: {ChangedAt}",
                notification.IncidentId,
                notification.PreviousStatus,
                notification.NewStatus,
                notification.ChangedById,
                notification.ChangedAt
            );

            // SDDC-15: In the future, the SignalR Hub (BroadcastIncidentStatusChanged) 
            // and external notification services will be called from this point.
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Error occurred while processing IncidentStatusChangedEvent for IncidentId: {IncidentId}", 
                notification.IncidentId
            );
        }

        return Task.CompletedTask;
    }
}
