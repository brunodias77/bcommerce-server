using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Sales.Orders.Validators;

public class OrderValidator : Validator
{
    private readonly Order _order;

    public OrderValidator(Order order, IValidationHandler handler) : base(handler)
    {
        _order = order;
    }

    public override void Validate()
    {
        if (_order.ClientId == Guid.Empty)
        {
            ValidationHandler.Append(new Error("'ClientId' do pedido é obrigatório."));
        }

        if (!_order.Items.Any())
        {
            ValidationHandler.Append(new Error("O pedido deve conter pelo menos um item."));
        }
        
        if (_order.ShippingAmount.Amount < 0)
        {
            ValidationHandler.Append(new Error("O valor do frete não pode ser negativo."));
        }
        
        if (_order.DiscountAmount.Amount < 0)
        {
            ValidationHandler.Append(new Error("O valor do desconto não pode ser negativo."));
        }
        
        // Validação de consistência interna
        var calculatedItemsTotal = _order.Items.Sum(i => i.LineItemTotalAmount.Amount);
        if (_order.ItemsTotalAmount.Amount != calculatedItemsTotal)
        {
            ValidationHandler.Append(new Error("O total dos itens do pedido está inconsistente."));
        }
    }
}