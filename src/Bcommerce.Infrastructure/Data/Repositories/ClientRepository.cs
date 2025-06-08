using Bcommerce.Domain.Abstractions;
using Bcommerce.Domain.Clients;
using Bcommerce.Domain.Clients.enums;
using Bcommerce.Domain.Clients.Repositories;
using Bcommerce.Infrastructure.Data.Models;
using Dapper;

namespace Bcommerce.Infrastructure.Data.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly IUnitOfWork _uow;

    public ClientRepository(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task Insert(Client aggregate, CancellationToken cancellationToken)
    {
        // SQL corrigido: removido o cast "::client_status_enum"
        const string sql = @"
                 INSERT INTO client (
                     client_id, first_name, last_name, email, email_verified_at, 
                     phone, password_hash, cpf, date_of_birth, newsletter_opt_in, 
                     status, created_at, updated_at, deleted_at
                 ) VALUES (
                     @Id, @FirstName, @LastName, @Email, @EmailVerified, 
                     @PhoneNumber, @PasswordHash, @Cpf, @DateOfBirth, @NewsletterOptIn, 
                     @StatusString, @CreatedAt, @UpdatedAt, @DeletedAt
                 )";
            
        var parameters = new
             {
                 aggregate.Id,
                 aggregate.FirstName,
                 aggregate.LastName,
                 aggregate.Email,
                 aggregate.EmailVerified,
                 aggregate.PhoneNumber, // Nome do parâmetro corresponde à propriedade
                 aggregate.PasswordHash,
                 aggregate.Cpf,
                 DateOfBirth = aggregate.DateOfBirth?.ToDateTime(TimeOnly.MinValue),
                 aggregate.NewsletterOptIn,
                 StatusString = aggregate.Status.ToString().ToLowerInvariant(),
                 aggregate.CreatedAt,
                 aggregate.UpdatedAt,
                 aggregate.DeletedAt
             };

             await _uow.Connection.ExecuteAsync(new CommandDefinition(
                 sql,
                 parameters,
                 transaction: _uow.Transaction,
                 cancellationToken: cancellationToken));
    }

    public async Task<Client?> GetByEmail(string email, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT 
                client_id, first_name, last_name, email, email_verified_at, 
                phone, password_hash, cpf, date_of_birth, newsletter_opt_in, 
                status, created_at, updated_at, deleted_at
            FROM client 
            WHERE email = @Email AND deleted_at IS NULL;";

        var model = await _uow.Connection.QuerySingleOrDefaultAsync<ClientDataModel>(
            sql, 
            new { Email = email },
            transaction: _uow.HasActiveTransaction ? _uow.Transaction : null
        );

        if (model == null) return null;

        // Hidrata a entidade de domínio a partir do modelo de dados
        return Client.With(
            model.client_id,
            model.first_name,
            model.last_name,
            model.email,
            model.email_verified_at,
            model.phone,
            model.password_hash,
            model.cpf,
            model.date_of_birth.HasValue ? DateOnly.FromDateTime(model.date_of_birth.Value) : null,
            model.newsletter_opt_in,
            Enum.Parse<ClientStatus>(model.status, true),
            model.created_at,
            model.updated_at,
            model.deleted_at
        );
    }

    public async Task<Client> Get(Guid id, CancellationToken cancellationToken)
    {
        const string sql = "SELECT * FROM client WHERE client_id = @Id AND deleted_at IS NULL";
        var model = await _uow.Connection.QuerySingleOrDefaultAsync<ClientDataModel>(
            sql, new { Id = id }, transaction: _uow.HasActiveTransaction ? _uow.Transaction : null
        );
        
        if (model == null) return null; // Ou lançar exceção ClientNotFoundException
        
        // Reutilize a lógica de hidratação do GetByEmail
        return Client.With(
            model.client_id, model.first_name, model.last_name, model.email, model.email_verified_at,
            model.phone, model.password_hash, model.cpf,
            model.date_of_birth.HasValue ? DateOnly.FromDateTime(model.date_of_birth.Value) : null,
            model.newsletter_opt_in, Enum.Parse<ClientStatus>(model.status, true),
            model.created_at, model.updated_at, model.deleted_at);    
    }

    public Task Delete(Client aggregate, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task Update(Client aggregate, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE client SET
                first_name = @FirstName,
                last_name = @LastName,
                email = @Email,
                email_verified_at = @EmailVerified,
                phone = @PhoneNumber,
                password_hash = @PasswordHash,
                status = @StatusString,
                updated_at = @UpdatedAt
            WHERE client_id = @Id";

        var parameters = new
        {
            aggregate.Id,
            aggregate.FirstName,
            aggregate.LastName,
            aggregate.Email,
            aggregate.EmailVerified,
            aggregate.PhoneNumber,
            aggregate.PasswordHash,
            StatusString = aggregate.Status.ToString().ToLowerInvariant(),
            aggregate.UpdatedAt
        };

        await _uow.Connection.ExecuteAsync(
            new CommandDefinition(sql, parameters, _uow.Transaction, cancellationToken: cancellationToken));
        
    }
}

