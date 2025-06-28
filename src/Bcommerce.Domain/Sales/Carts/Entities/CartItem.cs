using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Common;
using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Sales.Carts.Entities;

public class CartItem : Entity
{
    public Guid CartId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public int Quantity { get; private set; }
    public Money Price { get; private set; } // Preço no momento da adição
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private CartItem()
    {
    }

    internal static CartItem NewItem(Guid cartId, Guid productVariantId, int quantity, Money price)
    {
        DomainException.ThrowWhen(quantity <= 0, "A quantidade deve ser maior que zero.");

        return new CartItem
        {
            CartId = cartId,
            ProductVariantId = productVariantId,
            Quantity = quantity,
            Price = price,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static CartItem With(Guid id, Guid cartId, Guid productVariantId, int quantity, Money price,
        DateTime createdAt, DateTime updatedAt)
    {
        return new CartItem
        {
            Id = id,
            CartId = cartId,
            ProductVariantId = productVariantId,
            Quantity = quantity,
            Price = price,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    internal void IncreaseQuantity(int quantityToAdd)
    {
        DomainException.ThrowWhen(quantityToAdd <= 0, "A quantidade a ser adicionada deve ser positiva.");
        Quantity += quantityToAdd;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void UpdateQuantity(int newQuantity)
    {
        DomainException.ThrowWhen(newQuantity <= 0, "A nova quantidade deve ser maior que zero.");
        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public Money GetTotal() => Money.Create(Price.Amount * Quantity, Price.Currency);

    public override void Validate(IValidationHandler handler)
    {
        // Validações se necessárias
    }
}