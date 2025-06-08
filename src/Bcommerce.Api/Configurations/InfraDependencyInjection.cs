using Bcommerce.Domain.Abstractions;
using Bcommerce.Domain.Clients.Repositories;
using Bcommerce.Domain.Security;
using Bcommerce.Domain.Services;
using Bcommerce.Infrastructure.Data.Repositories;
using Bcommerce.Infrastructure.Events;
using Bcommerce.Infrastructure.Security;
using Bcommerce.Infrastructure.Services;

namespace Bcommerce.Api.Configurations;

public static class InfraDependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddRepositories(services);
        AddPasswordEncrypter(services, configuration);
        AddServices(services, configuration);
        AddEvents(services, configuration);
        AddLoggedUser(services, configuration);
        // AddLoggedCustomer(services, configuration);
        // AddToken(services, configuration);
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IUnitOfWork, DapperUnitOfWork>();
    }

    private static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEmailService, ConsoleEmailService>(); // Singleton para o serviço de console
        services.AddScoped<ITokenService, JwtTokenService>();
    }
    
    private static void AddPasswordEncrypter(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPasswordEncripter, PasswordEncripter>();
    }

    private static void AddEvents(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

    }

    private static void AddLoggedUser(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor(); 
        services.AddScoped<ILoggedUser, LoggedUser>();
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