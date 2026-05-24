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
        // En Azure (producción) o Docker Compose, recibimos la cadena base
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        var tenantId = string.IsNullOrEmpty(_tenantContext.TenantId) ? "Default" : _tenantContext.TenantId;
        
        if (string.IsNullOrEmpty(connectionString))
        {
            // Desarrollo local sin Docker
            connectionString = $@"Server=localhost\SQLEXPRESS;Database=BasErpBd_{tenantId};Integrated Security=True;TrustServerCertificate=True;";
        }
        else
        {
            // Entorno Dockerizado o Azure SQL: Modificamos dinámicamente la base de datos
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            
            // Tomamos el nombre base de la BD (ej. BasErpBd) y le añadimos el sufijo del tenant
            var baseDbName = builder.InitialCatalog.Split('_')[0];
            if (string.IsNullOrEmpty(baseDbName)) baseDbName = "BasErpBd";
            
            builder.InitialCatalog = $"{baseDbName}_{tenantId}";
            connectionString = builder.ConnectionString;
        }
        
        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Add configurations here
    }
}
