using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Sales.Carts.AddItemToCart;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Sales.Carts.GetCart;

public interface IGetCartUseCase : IUseCase<object?, CartOutput, Notification>
{
}