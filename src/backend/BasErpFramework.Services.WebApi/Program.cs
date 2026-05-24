using BasErpFramework.Domain.Core;
using BasErpFramework.Infrastructure.Repository;
using BasErpFramework.Application.Main;
using BasErpFramework.Services.WebApi.Modules.Swagger;
using BasErpFramework.Services.WebApi.Modules.Authentication;
using Serilog;
using BasErpFramework.Transversal.Logging;
using BasErpFramework.Services.WebApi.Middlewares;
using BasErpFramework.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using BasErpFramework.Application.Main.Hubs;
using BasErpFramework.Application.Interface;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Permite los puertos dinámicos de Aspire (ej. 59239)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddOpenApi();
builder.Services.AddDomainServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();
builder.Services.AddTransversalServices(builder.Configuration);

builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddSignalR();
builder.Host.UseSerilog();

builder.Services.AddAuth(builder.Configuration);
builder.Services.AddSwagger();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BasErpFramework API v1");
        c.RoutePrefix = "swagger"; // Set Swagger UI at the app's root
        c.DisplayRequestDuration(); // Show request duration in the UI
        c.EnableDeepLinking(); // Enable deep linking for tags and operations
        c.ShowExtensions();
    });
    //app.MapOpenApi();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();

app.UseMiddleware<TenantMiddleware>();

  // Auto-migrate default or known tenants in the background to avoid blocking Container App startup probe
  _ = Task.Run(() => 
  {
      try 
      {
          var tenants = new[] { "Default", "TenantA", "TenantB" };
          foreach (var t in tenants)
          {
              using var scope = app.Services.CreateScope();
              var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
              tenantContext.TenantId = t;
              
              using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
              context.Database.EnsureCreated();
              
              // Si la base de datos ya existía (ej. pre-creada por Bicep en Azure SQL) pero vacía,
              // EnsureCreated() no hace nada. Forzamos la creación de tablas con el IRelationalDatabaseCreator.
              try
              {
                  var databaseCreator = context.Database.GetService<IRelationalDatabaseCreator>();
                  databaseCreator.CreateTables();
              }
              catch (Exception ex)
              {
                  // Ignorar si las tablas ya existen (ej. cuando ya fueron creadas anteriormente)
                  Log.Warning("Intento de creación de tablas para inquilino {TenantId}: {Message}", t, ex.Message);
              }
          }
      }
      catch (Exception ex)
      {
          Log.Error(ex, "Error creating tenant databases in background");
      }
  });
app.UseAuthorization();

app.MapControllers();
app.MapHub<ProductoHub>("/hubs/producto");
app.MapDefaultEndpoints();

try
{
    Log.Information("Starting web host");
    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

