using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Sales.Carts;
using Bcommerce.Domain.Sales.Carts.Entities;
using Bcommerce.Domain.Sales.Carts.Repositories;
using Bcommerce.Infrastructure.Data.Models;
using Dapper;

namespace Bcommerce.Infrastructure.Data.Repositories;

public class CartRepository : ICartRepository
{
    private readonly IUnitOfWork _uow;

    public CartRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Cart?> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT c.*, ci.cart_item_id as Id, ci.*
            FROM carts c
            LEFT JOIN cart_items ci ON c.cart_id = ci.cart_id
            WHERE c.client_id = @ClientId;
        ";
        
        var cartDict = new Dictionary<Guid, Cart>();

        var queryResult = await _uow.Connection.QueryAsync<CartDataModel, CartItemDataModel, Cart>(
            sql,
            (cartData, cartItemData) =>
            {
                if (!cartDict.TryGetValue(cartData.cart_id, out var cart))
                {
                    cart = Cart.With(cartData.cart_id, cartData.client_id.Value, cartData.created_at, cartData.updated_at, new List<CartItem>());
                    cartDict.Add(cart.Id, cart);
                }

                if (cartItemData != null)
                {
                    var cartItem = CartItem.With(cartItemData.cart_item_id, cartItemData.cart_id, cartItemData.product_variant_id, cartItemData.quantity, Money.Create(cartItemData.unit_price, cartItemData.currency));
                    (cart.Items as List<CartItem>)?.Add(cartItem);
                }
                
                return cart;
            },
            new { ClientId = clientId },
            splitOn: "Id",
            transaction: _uow.HasActiveTransaction ? _uow.Transaction : null
        );

        return cartDict.Values.FirstOrDefault();
    }
    
    // O IRepository<T> exige estes métodos, vamos implementá-los.
    public async Task Insert(Cart aggregate, CancellationToken cancellationToken)
    {
        const string cartSql = @"
            INSERT INTO carts (cart_id, client_id, created_at, updated_at, expires_at)
            VALUES (@Id, @ClientId, @CreatedAt, @UpdatedAt, @ExpiresAt);
        ";
        await _uow.Connection.ExecuteAsync(new CommandDefinition(cartSql, new { aggregate.Id, aggregate.ClientId, aggregate.CreatedAt, aggregate.UpdatedAt, ExpiresAt = DateTime.UtcNow.AddDays(30) }, _uow.Transaction, cancellationToken: cancellationToken));
        
        await SyncCartItems(aggregate, cancellationToken);
    }

    public async Task Update(Cart aggregate, CancellationToken cancellationToken)
    {
        // Atualiza o carrinho principal (basicamente a data de modificação)
        const string cartSql = "UPDATE carts SET updated_at = @UpdatedAt WHERE cart_id = @Id;";
        await _uow.Connection.ExecuteAsync(new CommandDefinition(cartSql, new { aggregate.Id, aggregate.UpdatedAt }, _uow.Transaction, cancellationToken: cancellationToken));
        
        // Sincroniza os itens (adição/remoção/atualização)
        await SyncCartItems(aggregate, cancellationToken);
    }
    
    private async Task SyncCartItems(Cart aggregate, CancellationToken cancellationToken)
    {
        // Esta lógica de sincronização pode ser complexa. Para simplificar:
        // 1. Remove todos os itens existentes do banco de dados para este carrinho.
        // 2. Insere todos os itens atuais do agregado.
        // Esta abordagem é mais simples de implementar do que verificar item por item.
        
        const string deleteSql = "DELETE FROM cart_items WHERE cart_id = @CartId;";
        await _uow.Connection.ExecuteAsync(new CommandDefinition(deleteSql, new { CartId = aggregate.Id }, _uow.Transaction, cancellationToken: cancellationToken));

        const string insertSql = @"
            INSERT INTO cart_items (cart_item_id, cart_id, product_variant_id, quantity, unit_price, currency, created_at, updated_at)
            VALUES (@Id, @CartId, @ProductVariantId, @Quantity, @PriceAmount, @PriceCurrency, @CreatedAt, @UpdatedAt);
        ";
        foreach (var item in aggregate.Items)
        {
            await _uow.Connection.ExecuteAsync(new CommandDefinition(insertSql, new
            {
                item.Id, item.CartId, item.ProductVariantId, item.Quantity,
                PriceAmount = item.Price.Amount, PriceCurrency = item.Price.Currency,
                item.CreatedAt, item.UpdatedAt
            }, _uow.Transaction, cancellationToken: cancellationToken));
        }
    }

    // Métodos não implementados por agora, pois não são o foco principal.
    public Task<Cart?> Get(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task Delete(Cart aggregate, CancellationToken cancellationToken) => throw new NotImplementedException();
}