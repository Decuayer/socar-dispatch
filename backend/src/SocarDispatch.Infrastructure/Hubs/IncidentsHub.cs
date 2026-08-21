using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SocarDispatch.Infrastructure.Hubs;

[Authorize]
public class IncidentsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (role == "Operator")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "operators");
        }

        await base.OnConnectedAsync();
    }
}
