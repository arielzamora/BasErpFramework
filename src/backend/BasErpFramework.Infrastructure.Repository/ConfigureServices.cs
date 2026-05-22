using Microsoft.Extensions.DependencyInjection;
using BasErpFramework.Infrastructure.Interface; 
using BasErpFramework.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using BasErpFramework.Domain.Entity;    

namespace BasErpFramework.Infrastructure.Repository
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddSingleton<DapperContext>();
            services.AddScoped<IProductosRepository, ProductosRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            return services;
        }   
    }
}
