using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Customers.Clients.Repositories;
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
        // SQL corrigido para usar 'token_id' como nome da coluna e @Id como parâmetro,
        // pois Dapper irá mapear a propriedade 'Id' da entidade.
        const string sql = @"
            INSERT INTO email_verification_tokens (token_id, client_id, token_hash, expires_at, created_at)
            VALUES (@Id, @ClientId, @TokenHash, @ExpiresAt, @CreatedAt);
        ";
        await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, token, _uow.Transaction, cancellationToken: cancellationToken));
    }

    public async Task<EmailVerificationToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        const string sql = "SELECT * FROM email_verification_tokens WHERE token_hash = @TokenHash";
        
        // CORREÇÃO: Mapeia para o DataModel, não para a entidade de domínio.
        var model = await _uow.Connection.QuerySingleOrDefaultAsync<EmailVerificationTokenDataModel>(
            new CommandDefinition(sql, new { TokenHash = tokenHash }, _uow.Transaction, cancellationToken: cancellationToken)
        );

        return model is null ? null : Hydrate(model);
    }

    public async Task DeleteAsync(EmailVerificationToken token, CancellationToken cancellationToken)
    {
        // SQL corrigido para usar @Id como parâmetro.
        const string sql = "DELETE FROM email_verification_tokens WHERE token_id = @Id";
        await _uow.Connection.ExecuteAsync(
            new CommandDefinition(sql, new { token.Id }, _uow.Transaction, cancellationToken: cancellationToken)
        );
    }

    // MÉTODO ADICIONADO: Converte o DataModel para a Entidade de Domínio.
    private static EmailVerificationToken Hydrate(EmailVerificationTokenDataModel model)
    {
        return EmailVerificationToken.With(
            model.token_id,
            model.client_id,
            model.token_hash,
            model.expires_at,
            model.created_at
        );
    }
}