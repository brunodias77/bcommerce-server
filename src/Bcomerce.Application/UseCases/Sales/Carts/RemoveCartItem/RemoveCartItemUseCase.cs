using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Sales.Carts.AddItemToCart;
using Bcommerce.Domain.Sales.Carts.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcomerce.Application.UseCases.Sales.Carts.RemoveCartItem;

public class RemoveCartItemUseCase : IRemoveCartItemUseCase
{
    private readonly ILoggedUser _loggedUser;
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _uow;

    public RemoveCartItemUseCase(ILoggedUser loggedUser, ICartRepository cartRepository, IUnitOfWork uow)
    {
        _loggedUser = loggedUser;
        _cartRepository = cartRepository;
        _uow = uow;
    }
    public async Task<Result<CartOutput, Notification>> Execute(Guid cartItemId)
    {
        var clientId = _loggedUser.GetClientId();
        var cart = await _cartRepository.GetByClientIdAsync(clientId, CancellationToken.None);

        if (cart is null)
        {
            var notification = Notification.Create().Append(new Error("Carrinho não encontrado."));
            return Result<CartOutput, Notification>.Fail(notification);
        }
        
        cart.RemoveItem(cartItemId);
        
        await _uow.Begin();
        await _cartRepository.Update(cart, CancellationToken.None);
        await _uow.Commit();
        
        // Mapeia para o output
        var cartItemsOutput = cart.Items.Select(i => new CartItemOutput(i.Id, i.ProductVariantId, "Nome Fictício do Item", i.Quantity, i.Price.Amount, i.GetTotal().Amount)).ToList();
        var output = new CartOutput(cart.Id, cart.ClientId, cart.GetTotalPrice().Amount, cartItemsOutput);

        return Result<CartOutput, Notification>.Ok(output);
        
    }
}