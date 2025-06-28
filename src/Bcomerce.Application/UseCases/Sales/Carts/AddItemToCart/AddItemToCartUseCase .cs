using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Catalog.Products.Repositories;
using Bcommerce.Domain.Sales.Carts;
using Bcommerce.Domain.Sales.Carts.Repositories;
using Bcommerce.Domain.Services;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcomerce.Application.UseCases.Sales.Carts.AddItemToCart;

public class AddItemToCartUseCase : IAddItemToCartUseCase
{
    private readonly ILoggedUser _loggedUser;
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository; // Para buscar o preço do produto/variante
    private readonly IUnitOfWork _uow;

    public AddItemToCartUseCase(ILoggedUser loggedUser, ICartRepository cartRepository, IProductRepository productRepository, IUnitOfWork uow)
    {
        _loggedUser = loggedUser;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _uow = uow;
    }

    public async Task<Result<CartOutput, Notification>> Execute(AddItemToCartInput input)
    {
        var notification = Notification.Create();
        var clientId = _loggedUser.GetClientId();

        // 1. Encontrar o produto/variante para validar existência e obter o preço
        // Esta lógica precisa ser implementada no ProductRepository de forma mais granular.
        // Por simplicidade, vamos assumir que o produto existe e tem um preço fixo.
        // Em um cenário real, você buscaria a variante específica.
        var product = await _productRepository.Get(input.ProductVariantId, CancellationToken.None); // Simplificação: assumindo que o ID da variante é o ID do produto por enquanto.
        if (product is null)
        {
            notification.Append(new Error("Produto não encontrado."));
            return Result<CartOutput, Notification>.Fail(notification);
        }

        // 2. Obter o carrinho do cliente ou criar um novo
        var cart = await _cartRepository.GetByClientIdAsync(clientId, CancellationToken.None);
        bool isNewCart = cart is null;
        if (isNewCart)
        {
            cart = Cart.NewCart(clientId);
        }

        // 3. Adicionar o item ao agregado do carrinho
        // A lógica de negócio (somar quantidade se já existe, etc.) está dentro do agregado
        cart.AddItem(input.ProductVariantId, input.Quantity, product.BasePrice); // Usando o preço base do produto

        // 4. Persistir o carrinho (Insert ou Update)
        await _uow.Begin();
        try
        {
            if (isNewCart)
                await _cartRepository.Insert(cart, CancellationToken.None);
            else
                await _cartRepository.Update(cart, CancellationToken.None);

            await _uow.Commit();
        }
        catch (Exception)
        {
            await _uow.Rollback();
            notification.Append(new Error("Não foi possível adicionar o item ao carrinho."));
            return Result<CartOutput, Notification>.Fail(notification);
        }

        // 5. Mapear para o DTO de saída
        var cartItemsOutput = cart.Items.Select(i => new CartItemOutput(i.Id, i.ProductVariantId, "Nome do Produto", i.Quantity, i.Price.Amount, i.GetTotal().Amount)).ToList();
        var output = new CartOutput(cart.Id, cart.ClientId, cart.GetTotalPrice().Amount, cartItemsOutput);

        return Result<CartOutput, Notification>.Ok(output);
    }
}