using Bcommerce.Domain.Customers.Clients.Entities;

namespace Bcommerce.Domain.Customers.Clients.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken);
    Task<RefreshToken?> GetByTokenValueAsync(string tokenValue, CancellationToken cancellationToken);
    Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken);
}