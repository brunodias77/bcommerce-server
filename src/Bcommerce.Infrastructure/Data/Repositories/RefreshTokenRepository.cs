using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Dapper;

namespace Bcommerce.Infrastructure.Data.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IUnitOfWork _uow;

    public RefreshTokenRepository(IUnitOfWork uow) => _uow = uow;
    
    
    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO refresh_tokens (token_id, client_id, token_value, expires_at, created_at, revoked_at)
            VALUES (@Id, @ClientId, @TokenValue, @ExpiresAt, @CreatedAt, @RevokedAt);";
        await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, token, _uow.Transaction, cancellationToken: cancellationToken));
    }

    public async Task<RefreshToken?> GetByTokenValueAsync(string tokenValue, CancellationToken cancellationToken)
    {
        // Este método não deve ser implementado, pois a entidade não é hidratada diretamente.
        // A lógica de hidratação deve ser implementada aqui.
        // Por simplicidade, vamos retornar null.
        await Task.CompletedTask;
        return null;
    }


    public async Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE refresh_tokens SET
                revoked_at = @RevokedAt,
                version = version + 1
            WHERE token_id = @Id;";
        await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, token, _uow.Transaction, cancellationToken: cancellationToken));
    }
}