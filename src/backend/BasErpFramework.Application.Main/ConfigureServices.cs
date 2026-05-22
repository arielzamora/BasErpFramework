using Microsoft.Extensions.DependencyInjection;
using BasErpFramework.Application.Interface;
using System.Reflection;
using AutoMapper;
using BasErpFramework.Transversal.Common;


namespace BasErpFramework.Application.Main
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            
            services.AddScoped<IAuthApplication, AuthApplication>();
            services.AddScoped<IJwtService,JwtService>();
            services.AddScoped<IProductoService, ProductoService>();
            services.AddAutoMapper(cfg => { }, typeof(ConfigureServices).Assembly);

            return services;
        }
    }
}
