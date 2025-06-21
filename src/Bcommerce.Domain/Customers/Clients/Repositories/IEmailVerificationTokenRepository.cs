using Bcommerce.Domain.Customers.Clients.Entities;

namespace Bcommerce.Domain.Customers.Clients.Repositories;

public interface IEmailVerificationTokenRepository
{
    Task AddAsync(EmailVerificationToken token, CancellationToken cancellationToken);
    Task<EmailVerificationToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task DeleteAsync(EmailVerificationToken token, CancellationToken cancellationToken);
}