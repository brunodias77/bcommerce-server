using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Clients.AddAddress;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcomerce.Application.UseCases.Clients.UpdateAddress;

public interface IUpdateAddressUseCase : IUseCase<UpdateAddressInput, AddressOutput, Notification>
{
}