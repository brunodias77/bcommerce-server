using Bcommerce.Domain.Abstractions;

namespace Bcommerce.Domain.Clients.Repositories;

// Crie uma entidade simples para o token
public class EmailVerificationToken
{
    public Guid TokenId { get; set; }
    public Guid ClientId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public interface IEmailVerificationTokenRepository
{
    Task AddAsync(EmailVerificationToken token, CancellationToken cancellationToken);
    // NOVO: Para buscar o token pelo seu hash
    Task<EmailVerificationToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
    // NOVO: Para apagar o token depois de usado
    Task DeleteAsync(EmailVerificationToken token, CancellationToken cancellationToken);
}