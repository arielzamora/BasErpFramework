using BasErpFramework.Application.Interface;

namespace BasErpFramework.Application.Main;

public class TenantContext : ITenantContext
{
    public string TenantId { get; set; } = string.Empty;
}
