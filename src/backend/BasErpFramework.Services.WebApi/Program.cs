
using BasErpFramework.Domain.Core;
using BasErpFramework.Infrastructure.Repository;
using BasErpFramework.Application.Main;
using BasErpFramework.Services.WebApi.Modules.Swagger;
using BasErpFramework.Services.WebApi.Modules.Authentication;
using BasErpFramework.Transversal.Logging;
using Serilog;
using BasErpFramework.Transversal.Logging;
using BasErpFramework.Services.WebApi.Middlewares;
using BasErpFramework.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
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

app.UseAuthentication();

app.UseCors("CorsPolicy");

app.UseMiddleware<TenantMiddleware>();

// Auto-migrate default or known tenants (or at least initialize the model)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
    
    // Apply schema creation for a default tenant on startup
    tenantContext.TenantId = "Default";
    context.Database.EnsureCreated();
}
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

