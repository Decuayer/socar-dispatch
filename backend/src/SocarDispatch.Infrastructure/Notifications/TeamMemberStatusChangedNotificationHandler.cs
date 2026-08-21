using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SocarDispatch.Domain.Events;
using SocarDispatch.Infrastructure.Hubs;

namespace SocarDispatch.Infrastructure.Notifications;

public class TeamMemberStatusChangedNotificationHandler : INotificationHandler<TeamMemberStatusChangedEvent>
{
    private readonly IHubContext<IncidentsHub> _hubContext;
    private readonly ILogger<TeamMemberStatusChangedNotificationHandler> _logger;

    public TeamMemberStatusChangedNotificationHandler(
        IHubContext<IncidentsHub> hubContext,
        ILogger<TeamMemberStatusChangedNotificationHandler> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Handle(TeamMemberStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Team member status changed. TeamId: {TeamId}, UserId: {UserId}, NewStatus: {NewStatus}",
                notification.TeamId, notification.UserId, notification.NewStatus
            );

            // SignalR Operator Broadcast
            await _hubContext.Clients.Group("operators").SendAsync("MemberStatusChanged", new
            {
                teamId = notification.TeamId,
                userId = notification.UserId,
                previousStatus = notification.PreviousStatus.ToString(),
                newStatus = notification.NewStatus.ToString(),
                changedById = notification.ChangedById,
                changedAt = notification.ChangedAt
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TeamMemberStatusChangedNotificationHandler for TeamId: {TeamId}", notification.TeamId);
        }
    }
}
