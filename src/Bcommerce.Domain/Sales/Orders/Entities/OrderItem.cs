using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Common;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Sales.Orders.Entities;

public class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public string ItemSku { get; private set; }
    public string ItemName { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    
    public Money LineItemTotalAmount => Money.Create(UnitPrice.Amount * Quantity);

    private OrderItem() { }

    internal static OrderItem NewOrderItem(
        Guid orderId, Guid productVariantId, string itemSku,
        string itemName, int quantity, Money unitPrice)
    {
        return new OrderItem
        {
            OrderId = orderId,
            ProductVariantId = productVariantId,
            ItemSku = itemSku,
            ItemName = itemName,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
    
    public override void Validate(IValidationHandler handler) { }
}