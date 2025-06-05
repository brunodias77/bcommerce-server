using Bcommerce.Domain.Abstractions;
using Bcommerce.Domain.Clients;

namespace Bcommerce.Infrastructure.Data.Repositories;

public class ClientRepository : IRepository<Client>
{
    private readonly IUnitOfWork _uow;

    public ClientRepository(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }


    public Task Insert(Client aggregate, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Client> Get(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Delete(Client aggregate, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Update(Client aggregate, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}








//
// using Bcommerce.Domain.Abstractions;
// using Bcommerce.Domain.Entities.Clients;
// using Bcommerce.Domain.Entities.Clients.Enums;
// using Dapper;
// using System;
// using System.Data;
// using System.Threading;
// using System.Threading.Tasks;
//
// namespace bcommerce_server.Infra.Repositories.Clients // Ou um namespace mais específico para repositórios de cliente
// {
//     public class ClientRepository : IRepository<Client>
//     {
//         private readonly IUnitOfWork _uow;
//
//         public ClientRepository(IUnitOfWork uow)
//         {
//             _uow = uow ?? throw new ArgumentNullException(nameof(uow));
//         }
//
//         public async Task Insert(Client aggregate, CancellationToken cancellationToken)
//         {
//             const string sql = @"
//                 INSERT INTO client (
//                     client_id, first_name, last_name, email, email_verified_at, 
//                     phone, password_hash, cpf, date_of_birth, newsletter_opt_in, 
//                     status, created_at, updated_at, deleted_at
//                 ) VALUES (
//                     @Id, @FirstName, @LastName, @Email, @EmailVerifiedAt, 
//                     @Phone, @PasswordHash, @Cpf, @DateOfBirth, @NewsletterOptIn, 
//                     @StatusString::client_status_enum, @CreatedAt, @UpdatedAt, @DeletedAt
//                 );";
//
//             var parameters = new
//             {
//                 aggregate.Id,
//                 aggregate.FirstName,
//                 aggregate.LastName,
//                 aggregate.Email,
//                 aggregate.EmailVerifiedAt,
//                 aggregate.Phone,
//                 aggregate.PasswordHash,
//                 aggregate.Cpf,
//                 DateOfBirth = aggregate.DateOfBirth, // Assumindo Npgsql 6+ para DateOnly
//                 aggregate.NewsletterOptIn,
//                 StatusString = aggregate.Status.ToString().ToLowerInvariant(), // 'ativo', 'inativo', 'banido'
//                 aggregate.CreatedAt,
//                 aggregate.UpdatedAt,
//                 aggregate.DeletedAt
//             };
//
//             await _uow.Connection.ExecuteAsync(new CommandDefinition(
//                 sql,
//                 parameters,
//                 transaction: _uow.Transaction,
//                 cancellationToken: cancellationToken));
//         }
//
//         public async Task<Client?> Get(Guid id, CancellationToken cancellationToken)
//         {
//             const string sql = @"
//                 SELECT 
//                     client_id, first_name, last_name, email, email_verified_at, 
//                     phone, password_hash, cpf, date_of_birth, newsletter_opt_in, 
//                     status, created_at, updated_at, deleted_at
//                 FROM client
//                 WHERE client_id = @Id AND deleted_at IS NULL;";
//
//             var row = await _uow.Connection.QuerySingleOrDefaultAsync<ClientDataRow>(new CommandDefinition(
//                 sql,
//                 new { Id = id },
//                 transaction: _uow.Transaction, // Transação pode ser opcional para leituras
//                 cancellationToken: cancellationToken));
//
//             if (row == null)
//             {
//                 return null; // Ou lançar uma exceção ClientNotFoundException
//             }
//
//             // Mapeamento manual para Client.Hydrate
//             var clientStatus = Enum.Parse<ClientStatus>(row.status, ignoreCase: true);
//             
//             DateOnly? domainDateOfBirth = null;
//             if (row.date_of_birth.HasValue)
//             {
//                 // Se Npgsql < 6 ou se Dapper não mapear DATE para DateOnly diretamente,
//                 // row.date_of_birth será DateTime. Precisamos converter para DateOnly.
//                 // Se Npgsql 6+ estiver em uso, row.date_of_birth pode já ser DateOnly se o DTO for ajustado.
//                 // Por segurança, vamos assumir que é DateTime e converter.
//                 domainDateOfBirth = DateOnly.FromDateTime(row.date_of_birth.Value);
//             }
//
//             return Client.Hydrate(
//                 row.client_id,
//                 row.first_name,
//                 row.last_name,
//                 row.email,
//                 row.email_verified_at,
//                 row.phone,
//                 row.password_hash,
//                 row.cpf,
//                 domainDateOfBirth,
//                 row.newsletter_opt_in,
//                 clientStatus,
//                 row.created_at,
//                 row.updated_at,
//                 row.deleted_at
//             );
//         }
//
//         public async Task Update(Client aggregate, CancellationToken cancellationToken)
//         {
//             // A lógica do aggregate (ex: SoftDelete) já deve ter atualizado UpdatedAt
//             // e outros campos necessários (como Status, DeletedAt para soft delete).
//             const string sql = @"
//                 UPDATE client SET 
//                     first_name = @FirstName, 
//                     last_name = @LastName, 
//                     email = @Email, 
//                     email_verified_at = @EmailVerifiedAt, 
//                     phone = @Phone, 
//                     password_hash = @PasswordHash, 
//                     cpf = @Cpf, 
//                     date_of_birth = @DateOfBirth, 
//                     newsletter_opt_in = @NewsletterOptIn, 
//                     status = @StatusString::client_status_enum, 
//                     updated_at = @UpdatedAt, 
//                     deleted_at = @DeletedAt
//                 WHERE client_id = @Id AND deleted_at IS NULL;"; // Condição opcional de deleted_at IS NULL aqui
//
//              var parameters = new
//             {
//                 aggregate.Id,
//                 aggregate.FirstName,
//                 aggregate.LastName,
//                 aggregate.Email,
//                 aggregate.EmailVerifiedAt,
//                 aggregate.Phone,
//                 aggregate.PasswordHash,
//                 aggregate.Cpf,
//                 DateOfBirth = aggregate.DateOfBirth, // Assumindo Npgsql 6+ para DateOnly
//                 aggregate.NewsletterOptIn,
//                 StatusString = aggregate.Status.ToString().ToLowerInvariant(),
//                 aggregate.UpdatedAt, // Importante: o aggregate deve ter este campo atualizado
//                 aggregate.DeletedAt
//             };
//
//             var affectedRows = await _uow.Connection.ExecuteAsync(new CommandDefinition(
//                 sql,
//                 parameters,
//                 transaction: _uow.Transaction,
//                 cancellationToken: cancellationToken));
//
//             // Opcional: verificar affectedRows se for esperado que sempre afete 1 linha.
//             // if (affectedRows == 0) throw new Exception("Cliente não encontrado para atualização ou nenhuma alteração necessária.");
//         }
//
//
//         /// <summary>
//         /// Realiza um soft delete direto no banco de dados para o ID do agregado.
//         /// Esta abordagem não depende do estado do 'aggregate' passado, mas usa seu ID.
//         /// Se a intenção é persistir o estado de um aggregate que já teve SoftDelete() chamado,
//         /// então o método Update() seria mais apropriado após a chamada de SoftDelete() no aggregate.
//         /// </summary>
//         public async Task Delete(Client aggregate, CancellationToken cancellationToken)
//         {
//             // Este método realiza um soft delete diretamente no banco.
//             // Se o aggregate já teve seu método SoftDelete() chamado,
//             // então o método Update(aggregate) persistiria essas mudanças.
//             // Este Delete é mais direto para marcar como deletado no DB.
//             const string sql = @"
//                 UPDATE client SET 
//                     deleted_at = @Now,
//                     status = @StatusString::client_status_enum,
//                     updated_at = @Now
//                 WHERE client_id = @Id AND deleted_at IS NULL;";
//
//             var parameters = new
//             {
//                 Id = aggregate.Id,
//                 Now = DateTime.UtcNow,
//                 StatusString = ClientStatus.Inativo.ToString().ToLowerInvariant() // Define explicitamente como inativo
//             };
//
//             await _uow.Connection.ExecuteAsync(new CommandDefinition(
//                 sql,
//                 parameters,
//                 transaction: _uow.Transaction,
//                 cancellationToken: cancellationToken));
//         }
//     }
// }