using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository.Configuration;
using Repository.Interface;
using Service.Identity;
using Service.Implementation;
using Service.Interface;


namespace Service.Configuration;
public static class DependencyInjection
{
    public static void AddService(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterRepository(configuration);
        services.AddTransient<IIdentityService, IdentityService>();
        services.AddTransient<IUserManagerService, UserManagerService>();
        services.AddTransient<IMenuManagementService, MenuManagementService>();
        services.AddTransient<IErrorLogger, ErrorLogger>();
    }
}
