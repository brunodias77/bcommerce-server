using Bcommerce.Domain.Clients.Repositories;
using Bcommerce.Domain.Security;
using Bcommerce.Infrastructure.Data.Repositories;
using Bcommerce.Infrastructure.Security;

namespace Bcommerce.Api.Configurations;

public static class InfraDependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddRepositories(services);
        AddPasswordEncrypter(services, configuration);
        // AddLoggedCustomer(services, configuration);
        // AddToken(services, configuration);
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IUnitOfWork, DapperUnitOfWork>();

    }
    
    
    private static void AddPasswordEncrypter(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPasswordEncripter, PasswordEncripter>();
    }
    //
    // private static void AddLoggedCustomer(IServiceCollection services, IConfiguration configuration)
    // {
    //     services.AddScoped<ILoggedCustomer, LoggedCustomer>();
    // }
    
    // private static void AddToken(IServiceCollection services, IConfigurationManager configuration)
    // {
    //     services.Configure<JwtSettings>(configuration.GetSection("Settings:JwtSettings"));
    //     services.AddScoped<ITokenService, TokenService>();
    // }

}