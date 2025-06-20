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

    private CartItem() { }

    internal static CartItem NewItem(Guid cartId, Guid productVariantId, int quantity, Money price)
    {
        DomainException.ThrowWhen(quantity <= 0, "A quantidade deve ser maior que zero.");
        
        return new CartItem
        {
            CartId = cartId,
            ProductVariantId = productVariantId,
            Quantity = quantity,
            Price = price
        };
    }

    internal void IncreaseQuantity(int quantityToAdd)
    {
        DomainException.ThrowWhen(quantityToAdd <= 0, "A quantidade a ser adicionada deve ser positiva.");
        Quantity += quantityToAdd;
    }

    internal void UpdateQuantity(int newQuantity)
    {
        DomainException.ThrowWhen(newQuantity <= 0, "A nova quantidade deve ser maior que zero.");
        Quantity = newQuantity;
    }

    public Money GetTotal() => Money.Create(Price.Amount * Quantity, Price.Currency);

    public override void Validate(IValidationHandler handler)
    {
        // Validações se necessárias
    }
}