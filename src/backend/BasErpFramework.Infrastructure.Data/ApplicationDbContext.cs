using BasErpFramework.Application.Interface;
using BasErpFramework.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BasErpFramework.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    private readonly IConfiguration _configuration;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext, IConfiguration configuration)
        : base(options)
    {
        _tenantContext = tenantContext;
        _configuration = configuration;
        // NOTA: Se eliminó Database.EnsureCreated() del constructor por problemas de rendimiento 
        // y restricciones en Azure SQL. La inicialización ya se hace en Program.cs al arrancar.
    }

    public DbSet<Producto> Productos { get; set; }
    // User DbSet could be added here
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // En Azure (producción), Bicep inyecta la cadena completa en DefaultConnection
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        
        // Si no está DefaultConnection, asumimos desarrollo local (Multi-tenant SQLEXPRESS)
        if (string.IsNullOrEmpty(connectionString))
        {
            var tenantId = string.IsNullOrEmpty(_tenantContext.TenantId) ? "Default" : _tenantContext.TenantId;
            connectionString = $@"Server=localhost\SQLEXPRESS;Database=BasErpBd_{tenantId};Integrated Security=True;TrustServerCertificate=True;";
        }
        
        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Add configurations here
    }
}
