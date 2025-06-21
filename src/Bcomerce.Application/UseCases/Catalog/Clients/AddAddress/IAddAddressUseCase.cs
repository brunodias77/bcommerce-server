using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Clients.AddAddress;

public interface IAddAddressUseCase : IUseCase<AddAddressInput, AddressOutput, Notification>
{
}