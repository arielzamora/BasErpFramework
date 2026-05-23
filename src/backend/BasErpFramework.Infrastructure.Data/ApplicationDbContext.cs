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
        Database.EnsureCreated(); // Auto-crea la BD del tenant al vuelo
    }

    public DbSet<Producto> Productos { get; set; }
    // User DbSet could be added here
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var tenantId = string.IsNullOrEmpty(_tenantContext.TenantId) ? "Default" : _tenantContext.TenantId;
        
        // Obtenemos la conexión base inyectada por Aspire (apuntando al contenedor de SQL)
        var baseConnectionString = _configuration.GetConnectionString("sqlserver");
        
        // Si no se encuentra en Aspire, o queremos forzar el SQL local con Autenticación de Windows
        if (string.IsNullOrEmpty(baseConnectionString) || baseConnectionString.Contains("sa"))
        {
            // Apuntamos al SQL Server nativo de tu PC usando tu usuario de Windows y la instancia SQLEXPRESS
            baseConnectionString = @"Server=localhost\SQLEXPRESS;Integrated Security=True;TrustServerCertificate=True;";
        }
        
        // Aseguramos TrustServerCertificate para desarrollo local
        if (!baseConnectionString.Contains("TrustServerCertificate"))
        {
             baseConnectionString += ";TrustServerCertificate=True";
        }

        // Construimos el string final agregando el nombre de la BD dinámica
        var tenantConnectionString = $"{baseConnectionString};Database=BasErpBd_{tenantId}";
        
        optionsBuilder.UseSqlServer(tenantConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Add configurations here
    }
}
