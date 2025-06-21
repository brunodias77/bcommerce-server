using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Clients.AddAddress;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Clients.ListAddresses;

public interface IListMyAddressesUseCase : IUseCase<object, IEnumerable<AddressOutput>, Notification>
{
}