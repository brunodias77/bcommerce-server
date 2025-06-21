
using Bcomerce.Application.UseCases.Catalog.Clients.AddAddress;
using Bcomerce.Application.UseCases.Catalog.Clients.Create;
using Bcomerce.Application.UseCases.Catalog.Clients.DeleteAddress;
using Bcomerce.Application.UseCases.Catalog.Clients.GetMyProfile;
using Bcomerce.Application.UseCases.Catalog.Clients.ListAddresses;
using Bcomerce.Application.UseCases.Catalog.Clients.Login;
using Bcomerce.Application.UseCases.Catalog.Clients.UpdateAddress;
using Bcomerce.Application.UseCases.Catalog.Clients.VerifyEmail;
using Bcommerce.Application.Clients.Events;
using Bcommerce.Domain.Common;
using Bcommerce.Domain.Customers.Clients.Events;


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
        services.AddScoped<IUpdateAddressUseCase, UpdateAddressUseCase>();
        services.AddScoped<IDeleteAddressUseCase, DeleteAddressUseCase>();
    }

    private static void AddEvents(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDomainEventHandler<ClientCreatedEvent>, ClientCreatedEventHandler>();
    }

}