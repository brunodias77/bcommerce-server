namespace Bcommerce.Domain.Security;

public interface IRevokedTokenRepository
{
    /// <summary>
    /// Adiciona o ID de um token (jti) à lista de negação.
    /// </summary>
    Task AddAsync(Guid jti, Guid clientId, DateTime expiresAt, CancellationToken cancellationToken);

    /// <summary>
    /// Verifica se um ID de token (jti) está na lista de negação.
    /// </summary>
    Task<bool> IsTokenRevokedAsync(Guid jti, CancellationToken cancellationToken);
}