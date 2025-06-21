using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Sales.Carts.Repositories;

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken);
}