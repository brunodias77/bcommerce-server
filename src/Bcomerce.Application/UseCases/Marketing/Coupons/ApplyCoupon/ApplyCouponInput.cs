namespace Bcomerce.Application.UseCases.Marketing.Coupons.ApplyCoupon;

public record ApplyCouponInput(Guid OrderId, string CouponCode);
