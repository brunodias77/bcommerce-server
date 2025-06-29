using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Sales.Carts.AddItemToCart;
using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Sales.Carts.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcomerce.Application.UseCases.Sales.Carts.UpdateCartItemQuantity;

public class UpdateCartItemQuantityUseCase : IUpdateCartItemQuantityUseCase
{
    
    private readonly ILoggedUser _loggedUser;
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _uow;

    public UpdateCartItemQuantityUseCase(ILoggedUser loggedUser, ICartRepository cartRepository, IUnitOfWork uow)
    {
        _loggedUser = loggedUser;
        _cartRepository = cartRepository;
        _uow = uow;
    }
    public async Task<Result<CartOutput, Notification>> Execute(UpdateCartItemQuantityInput input)
    {
        var notification = Notification.Create();
        var clientId = _loggedUser.GetClientId();
        var cart = await _cartRepository.GetByClientIdAsync(clientId, CancellationToken.None);

        if (cart is null)
        {
            notification.Append(new Error("Carrinho não encontrado."));
            return Result<CartOutput, Notification>.Fail(notification);
        }
        try
        {
            cart.UpdateItemQuantity(input.CartItemId, input.NewQuantity);
        }
        catch (DomainException ex)
        {
            notification.Append(new Error(ex.Message));
            return Result<CartOutput, Notification>.Fail(notification);
        }
        
        await _uow.Begin();
        await _cartRepository.Update(cart, CancellationToken.None);
        await _uow.Commit();
        
        // Mapeia para o output
        var cartItemsOutput = cart.Items.Select(i => new CartItemOutput(i.Id, i.ProductVariantId, "Nome Fictício do Item", i.Quantity, i.Price.Amount, i.GetTotal().Amount)).ToList();
        var output = new CartOutput(cart.Id, cart.ClientId, cart.GetTotalPrice().Amount, cartItemsOutput);

        return Result<CartOutput, Notification>.Ok(output);
    }
}