using Bcommerce.Domain.Clients.Repositories;
using Bcommerce.Infrastructure.Data.Models;
using Dapper;

namespace Bcommerce.Infrastructure.Data.Repositories;

public class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly IUnitOfWork _uow;

    public EmailVerificationTokenRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task AddAsync(EmailVerificationToken token, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO email_verification_token (token_id, client_id, token_hash, expires_at, created_at)
            VALUES (@TokenId, @ClientId, @TokenHash, @ExpiresAt, @CreatedAt);
        ";
        await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, token, _uow.Transaction, cancellationToken: cancellationToken));
    }

    public async Task<EmailVerificationToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        const string sql = "SELECT * FROM email_verification_token WHERE token_hash = @TokenHash";
        return await _uow.Connection.QuerySingleOrDefaultAsync<EmailVerificationToken>(
            new CommandDefinition(sql, new { TokenHash = tokenHash }, _uow.Transaction, cancellationToken: cancellationToken)
        );   
    }

    public async Task DeleteAsync(EmailVerificationToken token, CancellationToken cancellationToken)
    {
        const string sql = "DELETE FROM email_verification_token WHERE token_id = @TokenId";
        await _uow.Connection.ExecuteAsync(
            new CommandDefinition(sql, new { token.TokenId }, _uow.Transaction, cancellationToken: cancellationToken)
        );    
    }
}