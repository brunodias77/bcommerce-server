using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcommerce.Api.Configurations;

public static class InfraDependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfigurationManager configuration)
    {
        AddRepositories(services);
        // AddPasswordEncrypter(services, configuration); // ✅ ADICIONE ESTA LINHA
        // AddLoggedCustomer(services, configuration);
        // AddToken(services, configuration);
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, DapperUnitOfWork>();

    }
    
    
    // private static void AddPasswordEncrypter(IServiceCollection services, IConfiguration configuration)
    // {
    //     services.AddScoped<IPasswordEncripter, PasswordEncripter>();
    // }
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