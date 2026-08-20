using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocarDispatch.Application.Common.Interfaces;
using SocarDispatch.Domain.Events;

namespace SocarDispatch.Infrastructure.Notifications;

public class AssignmentCreatedNotificationHandler : INotificationHandler<AssignmentCreatedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IPushNotificationService _pushService;
    private readonly ILogger<AssignmentCreatedNotificationHandler> _logger;

    public AssignmentCreatedNotificationHandler(
        IApplicationDbContext context,
        IPushNotificationService pushService,
        ILogger<AssignmentCreatedNotificationHandler> logger)
    {
        _context = context;
        _pushService = pushService;
        _logger = logger;
    }

    public async Task Handle(AssignmentCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Select the members of the assigned team who own the device token.
            var teamMembers = await _context.TeamMembers
                .Include(tm => tm.User)
                .Where(tm => tm.TeamId == notification.TeamId && tm.User.DeviceToken != null)
                .ToListAsync(cancellationToken);

            var tokens = teamMembers
                .Select(tm => tm.User.DeviceToken!)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .ToList();

            if (!tokens.Any())
            {
                _logger.LogInformation("No device tokens found for team members of TeamId: {TeamId}", notification.TeamId);
                return;
            }

            // 2. Query incident details from the database
            var incident = await _context.Incidents
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == notification.IncidentId, cancellationToken);

            if (incident == null)
            {
                _logger.LogWarning("Incident with ID {IncidentId} not found while preparing push notification.", notification.IncidentId);
                return;
            }

            // 3. Prepare high-priority payload data
            var data = new Dictionary<string, string>
            {
                { "incidentId", notification.IncidentId.ToString() },
                { "emergencyCode", incident.EmergencyCode },
                { "category", incident.Category },
                { "latitude", incident.Latitude.ToString(CultureInfo.InvariantCulture) },
                { "longitude", incident.Longitude.ToString(CultureInfo.InvariantCulture) },
                { "type", "DISPATCH_ALERT" }
            };

            // 4. Sending push notifications
            await _pushService.SendMulticastAsync(
                tokens,
                title: $"Acil Durum Atandı — {incident.EmergencyCode}",
                body: $"{incident.Category} | Konuma yönelin.",
                data: data,
                ct: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in AssignmentCreatedNotificationHandler for AssignmentId: {AssignmentId}", notification.AssignmentId);
        }
    }
}
