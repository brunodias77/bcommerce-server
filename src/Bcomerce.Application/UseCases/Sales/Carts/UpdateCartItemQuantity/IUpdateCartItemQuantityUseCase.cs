using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Sales.Carts.AddItemToCart;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Sales.Carts.UpdateCartItemQuantity;

public interface IUpdateCartItemQuantityUseCase : IUseCase<UpdateCartItemQuantityInput, CartOutput, Notification>
{
}