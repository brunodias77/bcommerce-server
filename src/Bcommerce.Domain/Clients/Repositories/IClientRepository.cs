using Bcommerce.Domain.Abstractions;

namespace Bcommerce.Domain.Clients.Repositories;

public interface IClientRepository : IRepository<Client>
{
    public Task<Client> GetByEmail(string email, CancellationToken cancellationToken);
}