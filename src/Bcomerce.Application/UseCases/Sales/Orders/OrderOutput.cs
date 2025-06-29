using Bcommerce.Domain.Sales.Orders;
using Bcommerce.Domain.Sales.Orders.Enums;

namespace Bcomerce.Application.UseCases.Sales.Orders;

public record OrderOutput(
    Guid OrderId,
    string ReferenceCode,
    OrderStatus Status,
    decimal TotalAmount
)
{
    public static OrderOutput FromOrder(Order order)
    {
        return new OrderOutput(
            order.Id,
            order.ReferenceCode,
            order.Status,
            order.GrandTotalAmount.Amount
        );
    }
}