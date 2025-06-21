using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Clients.AddAddress;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Clients.UpdateAddress;

public interface IUpdateAddressUseCase : IUseCase<UpdateAddressInput, AddressOutput, Notification>
{
}