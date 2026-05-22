using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace BasErpFramework.Transversal.Logging
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddTransversalServices(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "BasErpFramework")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                // .WriteTo.MSSqlServer(
                //     connectionString: configuration.GetConnectionString("DefaultConnection"),
                //     sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
                //     {
                //         TableName = "Logs",
                //         AutoCreateSqlTable = true
                //     },
                //     restrictedToMinimumLevel: LogEventLevel.Warning)
                .CreateLogger();

            services.AddSerilog();

            // Register custom logger
            services.AddScoped(typeof(IAppLogger<>), typeof(AppLogger<>));

            return services;
        }
    }
}
