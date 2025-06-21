using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Marketing.Coupons.Validators;

public class CouponValidator : Validator
{
    private readonly Coupon _coupon;

    public CouponValidator(Coupon coupon, IValidationHandler handler) : base(handler)
    {
        _coupon = coupon;
    }

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(_coupon.Code))
            ValidationHandler.Append(new Error("O código do cupom é obrigatório."));

        bool hasPercentage = _coupon.DiscountPercentage.HasValue;
        bool hasAmount = _coupon.DiscountAmount != null;

        if ((hasPercentage && hasAmount) || (!hasPercentage && !hasAmount))
            ValidationHandler.Append(new Error("O cupom deve ter ou um desconto percentual ou um valor de desconto fixo, mas não ambos."));

        if (hasPercentage && (_coupon.DiscountPercentage <= 0 || _coupon.DiscountPercentage > 100))
            ValidationHandler.Append(new Error("A porcentagem de desconto deve ser entre 0 e 100."));

        if (_coupon.ValidUntil <= _coupon.ValidFrom)
            ValidationHandler.Append(new Error("A data de validade final deve ser posterior à data de início."));
    }
}