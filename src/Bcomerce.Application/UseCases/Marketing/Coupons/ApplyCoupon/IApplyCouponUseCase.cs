using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Sales.Orders;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Marketing.Coupons.ApplyCoupon;

public interface IApplyCouponUseCase : IUseCase<ApplyCouponInput, OrderOutput, Notification>
{
}