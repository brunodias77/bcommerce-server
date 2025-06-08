using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Clients.AddAddress;
using Bcommerce.Domain.Clients.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcomerce.Application.UseCases.Clients.ListAddresses;

public class ListMyAddressesUseCase : IListMyAddressesUseCase
{
    private readonly ILoggedUser _loggedUser;
    private readonly IAddressRepository _addressRepository;

    public ListMyAddressesUseCase(ILoggedUser loggedUser, IAddressRepository addressRepository)
    {
        _loggedUser = loggedUser;
        _addressRepository = addressRepository;
    }

    public async Task<Result<IEnumerable<AddressOutput>, Notification>> Execute(object input)
    {
        var clientId = _loggedUser.GetClientId();
        var addresses = await _addressRepository.GetByClientIdAsync(clientId, CancellationToken.None);

        var output = addresses.Select(AddressOutput.FromAddress);

        return Result<IEnumerable<AddressOutput>, Notification>.Ok(output);
    }
}