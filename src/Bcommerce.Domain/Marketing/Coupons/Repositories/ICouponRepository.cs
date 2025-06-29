using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Marketing.Coupons.Repositories;

public interface ICouponRepository : IRepository<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken);
}