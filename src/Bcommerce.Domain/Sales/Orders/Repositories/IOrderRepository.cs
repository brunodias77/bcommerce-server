using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Sales.Orders.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    // Futuramente, podemos adicionar métodos específicos, como:
    // Task<IEnumerable<Order>> ListByClientIdAsync(Guid clientId, CancellationToken cancellationToken);
    // Task<Order?> GetByReferenceCodeAsync(string referenceCode, CancellationToken cancellationToken);
}