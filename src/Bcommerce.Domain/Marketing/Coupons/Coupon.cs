using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Common;
using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Marketing.Coupons.Enums;
using Bcommerce.Domain.Marketing.Coupons.Validators;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Marketing.Coupons;

public class Coupon : AggregateRoot
{
    public string Code { get; private set; }
    public string? Description { get; private set; }
    public decimal? DiscountPercentage { get; private set; }
    public Money? DiscountAmount { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidUntil { get; private set; }
    public int? MaxUses { get; private set; }
    public int TimesUsed { get; private set; }
    public Money? MinPurchaseAmount { get; private set; }
    public bool IsActive { get; private set; }
    public CouponType Type { get; private set; }
    public Guid? ClientId { get; private set; }

    private Coupon() { }

    public static Coupon NewPercentageCoupon(string code, decimal percentage, DateTime validFrom, DateTime validUntil, IValidationHandler handler, int? maxUses = null, Money? minPurchase = null, Guid? clientId = null)
    {
        var coupon = new Coupon
        {
            Code = code,
            DiscountPercentage = percentage,
            DiscountAmount = null,
            ValidFrom = validFrom,
            ValidUntil = validUntil,
            MaxUses = maxUses,
            MinPurchaseAmount = minPurchase,
            Type = clientId.HasValue ? CouponType.UserSpecific : CouponType.General,
            ClientId = clientId,
            IsActive = true,
            TimesUsed = 0
        };
        coupon.Validate(handler);
        return coupon;
    }

    public static Coupon NewAmountCoupon(string code, Money amount, DateTime validFrom, DateTime validUntil, IValidationHandler handler, int? maxUses = null, Money? minPurchase = null, Guid? clientId = null)
    {
        var coupon = new Coupon
        {
            Code = code,
            DiscountPercentage = null,
            DiscountAmount = amount,
            ValidFrom = validFrom,
            ValidUntil = validUntil,
            MaxUses = maxUses,
            MinPurchaseAmount = minPurchase,
            Type = clientId.HasValue ? CouponType.UserSpecific : CouponType.General,
            ClientId = clientId,
            IsActive = true,
            TimesUsed = 0
        };
        coupon.Validate(handler);
        return coupon;
    }

    public override void Validate(IValidationHandler handler)
    {
        new CouponValidator(this, handler).Validate();
    }

    public bool IsValid(Money orderTotal, Guid? orderClientId)
    {
        if (!IsActive) return false;
        if (DateTime.UtcNow < ValidFrom || DateTime.UtcNow > ValidUntil) return false;
        if (MaxUses.HasValue && TimesUsed >= MaxUses.Value) return false;
    
        // CORREÇÃO: Verifica se MinPurchaseAmount não é nulo antes de usar.
        if (MinPurchaseAmount != null && orderTotal.Amount < MinPurchaseAmount.Amount) return false;
    
        if (Type == CouponType.UserSpecific && ClientId != orderClientId) return false;

        return true;
    }

    public Money CalculateDiscount(Money orderTotal)
    {
        if (DiscountPercentage.HasValue)
        {
            return Money.Create(orderTotal.Amount * (DiscountPercentage.Value / 100));
        }
        if (DiscountAmount != null)
        {
            // Garante que o desconto não seja maior que o valor total da compra
            return Money.Create(Math.Min(orderTotal.Amount, DiscountAmount.Amount));
        }
        return Money.Create(0);
    }

    public void Use()
    {
        // CORREÇÃO: Adicionada a validação que lança a exceção.
        DomainException.ThrowWhen(MaxUses.HasValue && TimesUsed >= MaxUses.Value, "Este cupom já atingiu o limite máximo de usos.");
        TimesUsed++;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

// using Bcommerce.Domain.Catalog.Products.ValueObjects;
// using Bcommerce.Domain.Common;
// using Bcommerce.Domain.Exceptions;
// using Bcommerce.Domain.Marketing.Coupons.Enums;
// using Bcommerce.Domain.Marketing.Coupons.Validators;
// using Bcommerce.Domain.Validation;
//
// namespace Bcommerce.Domain.Marketing.Coupons;
//
// public class Coupon : AggregateRoot
// {
//     public string Code { get; private set; }
//     public string? Description { get; private set; }
//     public decimal? DiscountPercentage { get; private set; }
//     public Money? DiscountAmount { get; private set; }
//     public DateTime ValidFrom { get; private set; }
//     public DateTime ValidUntil { get; private set; }
//     public int? MaxUses { get; private set; }
//     public int TimesUsed { get; private set; }
//     public Money? MinPurchaseAmount { get; private set; }
//     public bool IsActive { get; private set; }
//     public CouponType Type { get; private set; }
//     public Guid? ClientId { get; private set; }
//
//     private Coupon() { }
//
//     public static Coupon NewPercentageCoupon(string code, decimal percentage, DateTime validFrom, DateTime validUntil, IValidationHandler handler, int? maxUses = null, Money? minPurchase = null, Guid? clientId = null)
//     {
//         var coupon = new Coupon
//         {
//             Code = code,
//             DiscountPercentage = percentage,
//             DiscountAmount = null,
//             ValidFrom = validFrom,
//             ValidUntil = validUntil,
//             MaxUses = maxUses,
//             MinPurchaseAmount = minPurchase,
//             Type = clientId.HasValue ? CouponType.UserSpecific : CouponType.General,
//             ClientId = clientId,
//             IsActive = true,
//             TimesUsed = 0
//         };
//         coupon.Validate(handler);
//         return coupon;
//     }
//
//     public static Coupon NewAmountCoupon(string code, Money amount, DateTime validFrom, DateTime validUntil, IValidationHandler handler, int? maxUses = null, Money? minPurchase = null, Guid? clientId = null)
//     {
//         var coupon = new Coupon
//         {
//             Code = code,
//             DiscountPercentage = null,
//             DiscountAmount = amount,
//             ValidFrom = validFrom,
//             ValidUntil = validUntil,
//             MaxUses = maxUses,
//             MinPurchaseAmount = minPurchase,
//             Type = clientId.HasValue ? CouponType.UserSpecific : CouponType.General,
//             ClientId = clientId,
//             IsActive = true,
//             TimesUsed = 0
//         };
//         coupon.Validate(handler);
//         return coupon;
//     }
//
//     public override void Validate(IValidationHandler handler)
//     {
//         new CouponValidator(this, handler).Validate();
//     }
//
//     public bool IsValid(Money orderTotal, Guid? orderClientId)
//     {
//         if (!IsActive) return false;
//         if (DateTime.UtcNow < ValidFrom || DateTime.UtcNow > ValidUntil) return false;
//         if (MaxUses.HasValue && TimesUsed >= MaxUses.Value) return false;
//     
//         // CORREÇÃO: Verifica se MinPurchaseAmount não é nulo antes de usar.
//         if (MinPurchaseAmount != null && orderTotal.Amount < MinPurchaseAmount.Amount) return false;
//     
//         if (Type == CouponType.UserSpecific && ClientId != orderClientId) return false;
//
//         return true;
//     }
//
//     public Money CalculateDiscount(Money orderTotal)
//     {
//         if (DiscountPercentage.HasValue)
//         {
//             return Money.Create(orderTotal.Amount * (DiscountPercentage.Value / 100));
//         }
//         if (DiscountAmount != null)
//         {
//             return Money.Create(Math.Min(orderTotal.Amount, DiscountAmount.Amount));
//         }
//         return Money.Create(0);
//     }
//
//     public void Use()
//     {
//         // CORREÇÃO: Adicionada a validação que lança a exceção.
//         DomainException.ThrowWhen(MaxUses.HasValue && TimesUsed >= MaxUses.Value, "Este cupom já atingiu o limite máximo de usos.");
//         TimesUsed++;
//     }
//
//     public void Deactivate() => IsActive = false;
//     public void Activate() => IsActive = true;
// }