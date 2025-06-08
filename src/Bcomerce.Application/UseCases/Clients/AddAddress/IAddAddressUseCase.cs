using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcomerce.Application.UseCases.Clients.AddAddress;

public interface IAddAddressUseCase : IUseCase<AddAddressInput, AddressOutput, Notification>
{
}