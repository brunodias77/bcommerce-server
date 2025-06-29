using Bcommerce.Domain.Sales.Orders;
using Bcommerce.Domain.Sales.Orders.Repositories;
using Dapper;

namespace Bcommerce.Infrastructure.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IUnitOfWork _uow;

    public OrderRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task Insert(Order aggregate, CancellationToken cancellationToken)
    {
        // 1. Inserir o registro principal na tabela 'orders'
        const string orderSql = @"
            INSERT INTO orders (order_id, reference_code, client_id, coupon_id, status, items_total_amount, discount_amount, shipping_amount, created_at, updated_at, version)
            VALUES (@Id, @ReferenceCode, @ClientId, @CouponId, @StatusString::order_status_enum, @ItemsTotalAmount, @DiscountAmount, @ShippingAmount, @CreatedAt, @UpdatedAt, 1);
        ";
        await _uow.Connection.ExecuteAsync(new CommandDefinition(orderSql, new
        {
            aggregate.Id,
            aggregate.ReferenceCode,
            aggregate.ClientId,
            aggregate.CouponId,
            StatusString = aggregate.Status.ToString().ToLower(),
            ItemsTotalAmount = aggregate.ItemsTotalAmount.Amount,
            DiscountAmount = aggregate.DiscountAmount.Amount,
            ShippingAmount = aggregate.ShippingAmount.Amount,
            aggregate.CreatedAt,
            aggregate.UpdatedAt
        }, _uow.Transaction, cancellationToken: cancellationToken));

        // 2. Inserir os itens do pedido
        const string itemsSql = @"
            INSERT INTO order_items (order_item_id, order_id, product_variant_id, item_sku, item_name, quantity, unit_price)
            VALUES (@Id, @OrderId, @ProductVariantId, @ItemSku, @ItemName, @Quantity, @UnitPrice);
        ";
        foreach (var item in aggregate.Items)
        {
            await _uow.Connection.ExecuteAsync(new CommandDefinition(itemsSql, new
            {
                item.Id,
                item.OrderId,
                item.ProductVariantId,
                item.ItemSku,
                item.ItemName,
                item.Quantity,
                UnitPrice = item.UnitPrice.Amount
            }, _uow.Transaction, cancellationToken: cancellationToken));
        }

        // 3. Inserir os endereços (snapshots)
        const string addressSql = @"
            INSERT INTO order_addresses (order_address_id, order_id, address_type, recipient_name, postal_code, street, street_number, complement, neighborhood, city, state_code, country_code, phone)
            VALUES (@Id, @OrderId, @AddressTypeString::address_type_enum, @RecipientName, @PostalCode, @Street, @Number, @Complement, @Neighborhood, @City, @StateCode, @CountryCode, @Phone);
        ";
        if (aggregate.ShippingAddress != null)
        {
            await _uow.Connection.ExecuteAsync(new CommandDefinition(addressSql, new {
                aggregate.ShippingAddress.Id, aggregate.ShippingAddress.OrderId, AddressTypeString = "shipping", aggregate.ShippingAddress.RecipientName, aggregate.ShippingAddress.PostalCode, aggregate.ShippingAddress.Street, aggregate.ShippingAddress.Number, aggregate.ShippingAddress.Complement, aggregate.ShippingAddress.Neighborhood, aggregate.ShippingAddress.City, aggregate.ShippingAddress.StateCode, aggregate.ShippingAddress.CountryCode, aggregate.ShippingAddress.Phone
            }, _uow.Transaction, cancellationToken: cancellationToken));
        }
        if (aggregate.BillingAddress != null)
        {
            await _uow.Connection.ExecuteAsync(new CommandDefinition(addressSql, new {
                aggregate.BillingAddress.Id, aggregate.BillingAddress.OrderId, AddressTypeString = "billing", aggregate.BillingAddress.RecipientName, aggregate.BillingAddress.PostalCode, aggregate.BillingAddress.Street, aggregate.BillingAddress.Number, aggregate.BillingAddress.Complement, aggregate.BillingAddress.Neighborhood, aggregate.BillingAddress.City, aggregate.BillingAddress.StateCode, aggregate.BillingAddress.CountryCode, aggregate.BillingAddress.Phone
            }, _uow.Transaction, cancellationToken: cancellationToken));
        }
    }
    
    public async Task Update(Order aggregate, CancellationToken cancellationToken)
    {
        const string sql = @"
        UPDATE orders SET
            coupon_id = @CouponId,
            status = @StatusString::order_status_enum,
            discount_amount = @DiscountAmount,
            updated_at = @UpdatedAt,
            version = version + 1
        WHERE order_id = @Id;
    ";
    
        await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            aggregate.Id,
            aggregate.CouponId,
            StatusString = aggregate.Status.ToString().ToLower(),
            DiscountAmount = aggregate.DiscountAmount.Amount,
            aggregate.UpdatedAt
        }, _uow.Transaction, cancellationToken: cancellationToken));
    }

    public Task<Order?> Get(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task Delete(Order aggregate, CancellationToken cancellationToken) => throw new NotImplementedException();
}