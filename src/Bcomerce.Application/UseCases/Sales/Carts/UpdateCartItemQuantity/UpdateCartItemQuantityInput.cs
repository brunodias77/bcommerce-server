namespace Bcomerce.Application.UseCases.Sales.Carts.UpdateCartItemQuantity;

public record UpdateCartItemQuantityInput(Guid CartItemId, int NewQuantity);
