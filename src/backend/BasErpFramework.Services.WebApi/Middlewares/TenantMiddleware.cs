using Microsoft.AspNetCore.Http;
using BasErpFramework.Application.Interface;
using System.Threading.Tasks;

namespace BasErpFramework.Services.WebApi.Middlewares;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantId))
        {
            tenantContext.TenantId = tenantId.ToString();
        }
        else if (context.Request.Query.TryGetValue("tenantId", out var queryTenantId))
        {
            // Fallback for SignalR WebSockets
            tenantContext.TenantId = queryTenantId.ToString();
        }
        else
        {
            // Default tenant or fallback logic could be added here
            tenantContext.TenantId = "Default";
        }

        await _next(context);
    }
}
