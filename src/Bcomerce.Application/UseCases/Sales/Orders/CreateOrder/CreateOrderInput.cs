namespace Bcomerce.Application.UseCases.Sales.Orders.CreateOrder;

public record CreateOrderInput(
    Guid ShippingAddressId,
    Guid BillingAddressId,
    decimal ShippingFee, // Valor do frete calculado no frontend
    string? Notes // Opcional: observações do cliente
);