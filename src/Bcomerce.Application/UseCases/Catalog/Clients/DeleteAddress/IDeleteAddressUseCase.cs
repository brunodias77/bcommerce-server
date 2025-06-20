using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcomerce.Application.UseCases.Clients.DeleteAddress;

public interface IDeleteAddressUseCase : IUseCase<DeleteAddressInput, bool, Notification>
{
}