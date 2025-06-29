using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Marketing.Coupons;
using Bcommerce.Domain.Marketing.Coupons.Enums;
using Bcommerce.Domain.Marketing.Coupons.Repositories;
using Bcommerce.Infrastructure.Data.Models;
using Dapper;

namespace Bcommerce.Infrastructure.Data.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly IUnitOfWork _uow;

    public CouponRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        const string sql = "SELECT * FROM coupons WHERE code = @Code AND deleted_at IS NULL AND is_active = TRUE;";
        var model = await _uow.Connection.QuerySingleOrDefaultAsync<CouponDataModel>(
            sql,
            new { Code = code },
            transaction: _uow.HasActiveTransaction ? _uow.Transaction : null
        );

        return model is null ? null : Hydrate(model);
    }
    
    public async Task Update(Coupon aggregate, CancellationToken cancellationToken)
    {
        // O principal campo a ser atualizado ao usar um cupom é 'times_used'.
        const string sql = @"
            UPDATE coupons SET
                times_used = @TimesUsed,
                is_active = @IsActive,
                updated_at = @UpdatedAt,
                version = version + 1
            WHERE coupon_id = @Id;
        ";
        await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, aggregate, _uow.Transaction, cancellationToken: cancellationToken));
    }

    private static Coupon Hydrate(CouponDataModel model)
    {
        // Este método de hidratação precisará ser implementado com base na sua entidade Coupon.
        // Por simplicidade, vamos usar um método de fábrica fictício.
        // Em um projeto real, você usaria um método "With" como nos outros agregados.
        var couponType = Enum.Parse<CouponType>(model.type, true);
        
        var coupon = model.discount_percentage.HasValue
            ? Coupon.NewPercentageCoupon(model.code, model.discount_percentage.Value, model.valid_from, model.valid_until, Bcommerce.Domain.Validation.Handlers.Notification.Create(), model.max_uses, null, model.client_id)
            : Coupon.NewAmountCoupon(model.code, Money.Create(model.discount_amount.Value), model.valid_from, model.valid_until, Bcommerce.Domain.Validation.Handlers.Notification.Create(), model.max_uses, null, model.client_id);
        
        // Simulação da reconstrução do estado
        typeof(Coupon).GetProperty("Id").SetValue(coupon, model.coupon_id);
        typeof(Coupon).GetProperty("TimesUsed").SetValue(coupon, model.times_used);

        return coupon;
    }

    public Task Insert(Coupon aggregate, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<Coupon?> Get(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task Delete(Coupon aggregate, CancellationToken cancellationToken) => throw new NotImplementedException();
}