using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SocarDispatch.Infrastructure.Hubs;

[Authorize]
public class LocationHub : Hub
{
    private static readonly ConcurrentDictionary<Guid, DateTime> _lastUpdateTimes = new();
    private const int ThrottleMs = 1000;

    public async Task StreamTeamLocation(Guid teamId, double lat, double lng)
    {
        var now = DateTime.UtcNow;
        if (_lastUpdateTimes.TryGetValue(teamId, out var last) && (now - last).TotalMilliseconds < ThrottleMs)
        {
            return; // Throttled: 1 saniyeden daha kısa süre içinde gelen harita verisini atla
        }

        _lastUpdateTimes[teamId] = now;
        await Clients.Group("operators").SendAsync("TeamLocationUpdated", new { teamId, lat, lng, timestamp = now });
    }
}
