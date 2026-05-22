using BasErpFramework.Application.Interface;
using BasErpFramework.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace BasErpFramework.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Producto> Productos { get; set; }
    // User DbSet could be added here
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var tenantId = string.IsNullOrEmpty(_tenantContext.TenantId) ? "Default" : _tenantContext.TenantId;
            optionsBuilder.UseInMemoryDatabase($"BasErpBd_{tenantId}");
        }
        else
        {
            // For explicitly configured options
            var tenantId = string.IsNullOrEmpty(_tenantContext.TenantId) ? "Default" : _tenantContext.TenantId;
            optionsBuilder.UseInMemoryDatabase($"BasErpBd_{tenantId}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Add configurations here
    }
}
