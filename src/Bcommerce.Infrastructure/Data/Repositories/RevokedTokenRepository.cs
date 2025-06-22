using Bcommerce.Domain.Security;
using Dapper;

namespace Bcommerce.Infrastructure.Data.Repositories;

public class RevokedTokenRepository : IRevokedTokenRepository
{
    private readonly IUnitOfWork _uow;

    public RevokedTokenRepository(IUnitOfWork uow) => _uow = uow;

    public async Task AddAsync(Guid jti, Guid clientId, DateTime expiresAt, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO revoked_tokens (jti, client_id, expires_at)
            VALUES (@Jti, @ClientId, @ExpiresAt);";
        
        // Não precisa de transação, pois é uma única operação de inserção.
        await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, new { Jti = jti, ClientId = clientId, ExpiresAt = expiresAt }, cancellationToken: cancellationToken));
    }

    public async Task<bool> IsTokenRevokedAsync(Guid jti, CancellationToken cancellationToken)
    {
        const string sql = "SELECT 1 FROM revoked_tokens WHERE jti = @Jti;";
        var result = await _uow.Connection.ExecuteScalarAsync<int?>(
            new CommandDefinition(sql, new { Jti = jti }, cancellationToken: cancellationToken)
        );
        return result.HasValue;
    }
}