using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validation.Handlers;

namespace  Bcomerce.Application.UseCases.Catalog.Clients.DeleteAddress;

public interface IDeleteAddressUseCase : IUseCase<DeleteAddressInput, bool, Notification>
{
}