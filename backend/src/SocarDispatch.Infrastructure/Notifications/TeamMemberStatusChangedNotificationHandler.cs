using MediatR;
using Microsoft.Extensions.Logging;
using SocarDispatch.Domain.Events;

namespace SocarDispatch.Infrastructure.Notifications;

/// Notification handler executed when a TeamMemberStatusChangedEvent is raised.
/// Logs the team member status change and provides the infrastructure for SDDC-15 / SignalR live operator map broadcasts.
public class TeamMemberStatusChangedNotificationHandler : INotificationHandler<TeamMemberStatusChangedEvent>
{
    private readonly ILogger<TeamMemberStatusChangedNotificationHandler> _logger;

    public TeamMemberStatusChangedNotificationHandler(ILogger<TeamMemberStatusChangedNotificationHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TeamMemberStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Team member status changed notification received. TeamId: {TeamId}, UserId: {UserId}, PreviousStatus: {PreviousStatus}, NewStatus: {NewStatus}, ChangedById: {ChangedById}, ChangedAt: {ChangedAt}",
                notification.TeamId,
                notification.UserId,
                notification.PreviousStatus,
                notification.NewStatus,
                notification.ChangedById,
                notification.ChangedAt
            );

            // SDDC-15 Link: Operatör haritası için SignalR Hub yayını (BroadcastTeamMemberStatusChanged)
            // ve push bildirim entegrasyonu bu noktadan çağrılacaktır.
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while processing TeamMemberStatusChangedEvent for TeamId: {TeamId}, UserId: {UserId}",
                notification.TeamId,
                notification.UserId
            );
        }

        return Task.CompletedTask;
    }
}
