using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Common;
using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Sales.Carts.Entities;
using Bcommerce.Domain.Sales.Orders.Entities;
using Bcommerce.Domain.Sales.Orders.Enums;
using Bcommerce.Domain.Sales.Orders.Validators;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Sales.Orders;

public class Order : AggregateRoot
{
    public string ReferenceCode { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid? CouponId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money ItemsTotalAmount { get; private set; }
    public Money DiscountAmount { get; private set; }
    public Money ShippingAmount { get; private set; }
    public Money GrandTotalAmount => ItemsTotalAmount - DiscountAmount + ShippingAmount;

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    
    private Order() { }

    public static Order NewOrder(
        Guid clientId, 
        IEnumerable<CartItem> cartItems, 
        Money shippingAmount, 
        IValidationHandler validationHandler) // Adicionado validationHandler
    {
        var order = new Order
        {
            ClientId = clientId,
            ReferenceCode = GenerateOrderCode(),
            Status = OrderStatus.Pending,
            ShippingAmount = shippingAmount,
            DiscountAmount = Money.Create(0)
        };

        foreach (var cartItem in cartItems)
        {
            // Em um cenário real, o SKU e o Nome seriam buscados do produto
            var orderItem = OrderItem.NewOrderItem(order.Id, cartItem.ProductVariantId, "SKU-TEMP", "Nome-TEMP", cartItem.Quantity, cartItem.Price);
            order._items.Add(orderItem);
        }
        
        order.RecalculateItemsTotal();
        
        // Valida o estado final do novo pedido
        order.Validate(validationHandler);

        // Dispara o evento apenas se o pedido for válido
        if (!validationHandler.HasError())
        {
            // order.RaiseEvent(new OrderCreatedEvent(order.Id, order.ReferenceCode));
        }

        return order;
    }

    // Adicionado método With para reconstrução
    public static Order With(
        Guid id, string referenceCode, Guid clientId, OrderStatus status,
        Money itemsTotal, Money discount, Money shipping, IEnumerable<OrderItem> items)
    {
        var order = new Order
        {
            Id = id,
            ReferenceCode = referenceCode,
            ClientId = clientId,
            Status = status,
            ItemsTotalAmount = itemsTotal,
            DiscountAmount = discount,
            ShippingAmount = shipping
        };
        order._items.AddRange(items);
        return order;
    }

    private void RecalculateItemsTotal()
    {
        ItemsTotalAmount = Money.Create(_items.Sum(i => i.LineItemTotalAmount.Amount));
    }
    
    // --- MÉTODOS DE TRANSIÇÃO DE ESTADO ---
    
    public void SetAsProcessing()
    {
        DomainException.ThrowWhen(Status != OrderStatus.Pending, "Somente pedidos pendentes podem ser movidos para processamento.");
        Status = OrderStatus.Processing;
    }

    public void Ship()
    {
        DomainException.ThrowWhen(Status != OrderStatus.Processing, "Somente pedidos em processamento podem ser enviados.");
        Status = OrderStatus.Shipped;
    }

    public void Deliver()
    {
        DomainException.ThrowWhen(Status != OrderStatus.Shipped, "Somente pedidos enviados podem ser marcados como entregues.");
        Status = OrderStatus.Delivered;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Delivered || Status == OrderStatus.Canceled)
        {
            throw new DomainException($"Não é possível cancelar um pedido com status '{Status}'.");
        }
        Status = OrderStatus.Canceled;
    }

    private static string GenerateOrderCode()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }

    // Método Validate agora implementado corretamente
    public override void Validate(IValidationHandler handler)
    {
        new OrderValidator(this, handler).Validate();
    }
}