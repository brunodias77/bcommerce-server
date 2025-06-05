using Bcomerce.Application.UseCases.Clients.Create;

namespace Bcommerce.Api.Configurations;

public static class ApplicationDependencyInjection
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        AddUseCases(services);
    }
    
    private static void AddUseCases(IServiceCollection services)
    {
        // Auth
        services.AddScoped<ICreateClientUseCase,  CreateClientUseCase>();
    }

}