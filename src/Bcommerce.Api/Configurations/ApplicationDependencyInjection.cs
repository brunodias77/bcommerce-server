using Bcomerce.Application.UseCases.Clients.AddAddress;
using Bcomerce.Application.UseCases.Clients.Create;
using Bcomerce.Application.UseCases.Clients.GetMyProfile;
using Bcomerce.Application.UseCases.Clients.ListAddresses;
using Bcomerce.Application.UseCases.Clients.Login;
using Bcomerce.Application.UseCases.Clients.VerifyEmail;
using Bcommerce.Application.Clients.Events;
using Bcommerce.Domain.Abstractions;
using Bcommerce.Domain.Clients.Events;

namespace Bcommerce.Api.Configurations;

public static class ApplicationDependencyInjection
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        AddUseCases(services);
        AddEvents(services, configuration);
    }
    
    private static void AddUseCases(IServiceCollection services)
    {
        // Auth
        services.AddScoped<ICreateClientUseCase,  CreateClientUseCase>();
        services.AddScoped<IVerifyEmailUseCase, VerifyEmailUseCase>();
        services.AddScoped<ILoginClientUseCase, LoginClientUseCase>();
        services.AddScoped<IGetMyProfileUseCase, GetMyProfileUseCase>();
        services.AddScoped<IAddAddressUseCase, AddAddressUseCase>();
        services.AddScoped<IListMyAddressesUseCase, ListMyAddressesUseCase>();
    }

    private static void AddEvents(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDomainEventHandler<ClientCreatedEvent>, ClientCreatedEventHandler>();
        
        // Precisa de uma implementação para IDomainEventPublisher
        // services.AddScoped<IDomainEventPublisher, MediatRDomainEventPublisher>();
    }

}