using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Sales.Carts.AddItemToCart;
using Bcommerce.Domain.Sales.Carts;
using Bcommerce.Domain.Sales.Carts.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcomerce.Application.UseCases.Sales.Carts.GetCart;

public class GetCartUseCase : IGetCartUseCase
{
    public GetCartUseCase(ILoggedUser loggedUser, ICartRepository cartRepository, IUnitOfWork uow)
    {
        _loggedUser = loggedUser;
        _cartRepository = cartRepository;
        _uow = uow;
    }

    private readonly ILoggedUser _loggedUser;
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _uow;

    public async Task<Result<CartOutput, Notification>> Execute(object? input)
    {
        var clientId = _loggedUser.GetClientId();
        var cart = await _cartRepository.GetByClientIdAsync(clientId, CancellationToken.None);
        // Se o cliente nao tem carrinho, crie um novo
        if (cart is null)
        {
            cart = Cart.NewCart(clientId);
            await _uow.Begin();
            await _cartRepository.Insert(cart, CancellationToken.None);
            await _uow.Commit();
        }
        // Em um cenário real, você buscaria os nomes dos produtos aqui para popular o DTO.
        // Por simplicidade, usaremos "Nome Fictício".
        var cartItemsOutput = cart.Items.Select(i => new CartItemOutput(
            i.Id,
            i.ProductVariantId,
            "Nome Fictício do Item", // TODO: Buscar nome real do produto/variante
            i.Quantity,
            i.Price.Amount,
            i.GetTotal().Amount
        )).ToList();
        
        var output = new CartOutput(cart.Id, cart.ClientId, cart.GetTotalPrice().Amount, cartItemsOutput);

        return Result<CartOutput, Notification>.Ok(output);
    }
}