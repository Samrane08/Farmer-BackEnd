using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace UserService.Helper;


public static class AuthenticationExtension
{
    public static void AddTokenAuthentication(this IServiceCollection services, IConfigurationSection config)
    {
        services
           .AddAuthentication()
           .AddJwtBearer(cfg =>
           {
               cfg.RequireHttpsMetadata = false;
               cfg.SaveToken = true;
               cfg.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidIssuer = config[nameof(JwtIssuerOptions.ValidIssuer)],
                   ValidAudience = config[nameof(JwtIssuerOptions.ValidAudience)],
                   ValidateIssuer = true,
                   ValidateAudience = true,
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config[nameof(JwtIssuerOptions.IssuerSigningKey)])),
                   ValidateLifetime = true,
                   ClockSkew = TimeSpan.Zero
               };
           });
    }
}

