using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Clients;
using Bcommerce.Domain.Clients.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validations.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcomerce.Application.UseCases.Clients.AddAddress;

public class AddAddressUseCase : IAddAddressUseCase
{
    public AddAddressUseCase(ILoggedUser loggedUser, IAddressRepository addressRepository, IUnitOfWork uow)
    {
        _loggedUser = loggedUser;
        _addressRepository = addressRepository;
        _uow = uow;
    }

    private readonly ILoggedUser _loggedUser;
    private readonly IAddressRepository _addressRepository;
    private readonly IUnitOfWork _uow;   
    public async Task<Result<AddressOutput, Notification>> Execute(AddAddressInput input)
    {
        var clientId = _loggedUser.GetClientId();
        var notification = Notification.Create();
        
        var address = Address.NewAddress(
            clientId,
            input.Type,
            input.PostalCode,
            input.Street,
            input.Number,
            input.Complement,
            input.Neighborhood,
            input.City,
            input.StateCode,
            input.IsDefault,
            notification
        );
        
        if (notification.HasError())
        {
            return Result<AddressOutput, Notification>.Fail(notification);
        }
        
        // Lógica para garantir que apenas um endereço seja o padrão (opcional)
        if (address.IsDefault)
        {
            // Aqui você buscaria outros endereços do cliente e os marcaria como não-padrão.
            // Isso requer uma transação mais complexa.
        }
        await _uow.Begin();

        try
        {
            await _addressRepository.AddAsync(address, CancellationToken.None);
            await _uow.Commit();
        }
        catch (Exception e)
        {
            await _uow.Rollback();
            notification.Append(new Bcommerce.Domain.Validations.Error("Erro ao salvar o endereço."));
            return Result<AddressOutput, Notification>.Fail(notification);
        }

        var output = AddressOutput.FromAddress(address);
        return Result<AddressOutput, Notification>.Ok(output);
        
    }
}