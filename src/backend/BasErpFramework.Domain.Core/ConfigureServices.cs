using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using BasErpFramework.Domain.Interface;

namespace BasErpFramework.Domain.Core
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {

            services.AddScoped<IUserDomain, UsersDomain>();
            return services;
        }
    }
}
