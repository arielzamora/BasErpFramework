using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BasErpFramework.Services.WebApi.Modules.Authentication
{
    public static class AuthenticationExtension
    {

        public static IServiceCollection AddAuth(this IServiceCollection services,IConfiguration configuration)
        {
         var jwtSettings = configuration.GetSection("Jwt");
          services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            return services;
        }

    }
}
