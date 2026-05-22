using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BasErpFramework.Application.Main.Hubs;

public class ProductoHub : Hub
{
    public async Task JoinTenantGroup(string tenantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Tenant_{tenantId}");
    }

    public async Task LeaveTenantGroup(string tenantId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Tenant_{tenantId}");
    }
}
