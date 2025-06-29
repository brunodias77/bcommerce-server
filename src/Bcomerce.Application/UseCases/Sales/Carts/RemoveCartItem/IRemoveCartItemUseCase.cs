using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Sales.Carts.AddItemToCart;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Sales.Carts.RemoveCartItem;

public interface IRemoveCartItemUseCase : IUseCase<Guid, CartOutput, Notification>
{
}
