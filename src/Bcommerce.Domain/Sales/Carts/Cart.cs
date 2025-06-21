using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Common;
using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Sales.Carts.Entities;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Sales.Carts;

public class Cart : AggregateRoot
{
    public Guid ClientId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    private readonly List<CartItem> _items = new();
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
    
    private Cart() { }

    public static Cart NewCart(Guid clientId)
    {
        DomainException.ThrowWhen(clientId == Guid.Empty, "ClientId é obrigatório.");
        return new Cart
        {
            ClientId = clientId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static Cart With(Guid id, Guid clientId, DateTime createdAt, DateTime updatedAt, IEnumerable<CartItem> items)
    {
        var cart = new Cart
        {
            Id = id,
            ClientId = clientId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
        };
        cart._items.AddRange(items);
        return cart;
    }
    
    public override void Validate(IValidationHandler handler)
    {
        // Validador se necessário
    }
    
    public Money GetTotalPrice()
    {
        var total = Money.Create(0);
        foreach (var item in _items)
        {
            total += item.GetTotal();
        }
        return total;
    }
    
    public void AddItem(Guid productVariantId, int quantity, Money price)
    {
        var existingItem = _items.FirstOrDefault(i => i.ProductVariantId == productVariantId);

        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            var newItem = CartItem.NewItem(this.Id, productVariantId, quantity, price);
            _items.Add(newItem);
        }
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RemoveItem(Guid cartItemId)
    {
        var itemToRemove = _items.FirstOrDefault(i => i.Id == cartItemId);
        if (itemToRemove != null)
        {
            _items.Remove(itemToRemove);
            UpdatedAt = DateTime.UtcNow;
        }
    }
    
    public void UpdateItemQuantity(Guid cartItemId, int newQuantity)
    {
        var itemToUpdate = _items.FirstOrDefault(i => i.Id == cartItemId);
        DomainException.ThrowWhen(itemToUpdate is null, "Item não encontrado no carrinho.");

        if (newQuantity == 0)
        {
            RemoveItem(cartItemId);
        }
        else
        {
            itemToUpdate.UpdateQuantity(newQuantity);
            UpdatedAt = DateTime.UtcNow;
        }
    }
    
    public void Clear()
    {
        _items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

}