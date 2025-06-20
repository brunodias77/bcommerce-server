using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Clients.AddAddress;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcomerce.Application.UseCases.Clients.ListAddresses;

public interface IListMyAddressesUseCase : IUseCase<object, IEnumerable<AddressOutput>, Notification>
{
}