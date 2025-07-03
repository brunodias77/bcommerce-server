using Bcommerce.Domain.Common;
using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Customers.Clients.Enums;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Customers.Consents;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Mappers;
using Bcommerce.Infrastructure.Data.Models;
using Dapper;

namespace Bcommerce.Infrastructure.Data.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly IUnitOfWork _uow;

    public ClientRepository(IUnitOfWork uow) => _uow = uow;

    public async Task Insert(Client aggregate, CancellationToken cancellationToken)
    {
        const string clientSql = @"
            INSERT INTO clients (client_id, first_name, last_name, email, email_verified_at, phone, password_hash, cpf, date_of_birth, newsletter_opt_in, status, role, created_at, updated_at, version)
            VALUES (@Id, @FirstName, @LastName, @EmailValue, @EmailVerified, @PhoneNumber, @PasswordHash, @CpfValue, @DateOfBirth, @NewsletterOptIn, @StatusString, @RoleString::user_role_enum, @CreatedAt, @UpdatedAt, 1);
        ";

        await _uow.Connection.ExecuteAsync(new CommandDefinition(clientSql, new
        {
            aggregate.Id,
            aggregate.FirstName,
            aggregate.LastName,
            EmailValue = aggregate.Email.Value,
            aggregate.EmailVerified,
            aggregate.PhoneNumber,
            aggregate.PasswordHash,
            CpfValue = aggregate.Cpf?.Value,
            DateOfBirth = aggregate.DateOfBirth?.ToDateTime(TimeOnly.MinValue),
            aggregate.NewsletterOptIn,
            StatusString = aggregate.Status.ToDbString(),
            RoleString = aggregate.Role.ToString().ToLower(),
            aggregate.CreatedAt,
            aggregate.UpdatedAt
        }, _uow.Transaction, cancellationToken: cancellationToken));

        await InsertAddresses(aggregate.Addresses, cancellationToken);
        await InsertSavedCards(aggregate.SavedCards, cancellationToken);
        await InsertConsents(aggregate.Consents, cancellationToken);
    }

    public async Task Update(Client aggregate, CancellationToken cancellationToken)
    {
        const string clientSql = @"
            UPDATE clients SET
                first_name = @FirstName,
                last_name = @LastName,
                email = @EmailValue,
                email_verified_at = @EmailVerified,
                phone = @PhoneNumber,
                password_hash = @PasswordHash,
                cpf = @CpfValue,
                date_of_birth = @DateOfBirth,
                newsletter_opt_in = @NewsletterOptIn,
                status = @StatusString,
                role = @RoleString::user_role_enum,
                failed_login_attempts = @FailedLoginAttempts,
                account_locked_until = @AccountLockedUntil,
                updated_at = @UpdatedAt,
                deleted_at = @DeletedAt,
                version = version + 1
            WHERE client_id = @Id;
        ";

        await _uow.Connection.ExecuteAsync(new CommandDefinition(clientSql, new
        {
            aggregate.Id,
            aggregate.FirstName,
            aggregate.LastName,
            EmailValue = aggregate.Email.Value,
            aggregate.EmailVerified,
            aggregate.PhoneNumber,
            aggregate.PasswordHash,
            CpfValue = aggregate.Cpf?.Value,
            DateOfBirth = aggregate.DateOfBirth?.ToDateTime(TimeOnly.MinValue),
            aggregate.NewsletterOptIn,
            StatusString = aggregate.Status.ToDbString(),
            RoleString = aggregate.Role.ToString().ToLower(),
            aggregate.FailedLoginAttempts,    
            aggregate.AccountLockedUntil,
            aggregate.UpdatedAt,
            aggregate.DeletedAt
        }, _uow.Transaction, cancellationToken: cancellationToken));
    }
        
    private const string GetClientBaseSql = "SELECT * FROM clients WHERE deleted_at IS NULL";

    public async Task<Client?> Get(Guid id, CancellationToken cancellationToken)
    {
        var sql = $"{GetClientBaseSql} AND client_id = @Id;";
        return await QueryAndHydrateClient(sql, new { Id = id });
    }

    public async Task<Client?> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var sql = $"{GetClientBaseSql} AND email = @Email;";
        return await QueryAndHydrateClient(sql, new { Email = email });
    }

    public async Task Delete(Client aggregate, CancellationToken cancellationToken)
    {
        const string sql = "UPDATE clients SET deleted_at = @Now, status = 'inativo' WHERE client_id = @Id;";
        await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, new { aggregate.Id, Now = DateTime.UtcNow }, _uow.Transaction, cancellationToken: cancellationToken));
    }

    private async Task<Client?> QueryAndHydrateClient(string sql, object param)
    {
        // *** CORREÇÃO APLICADA AQUI ***
        var transaction = _uow.HasActiveTransaction ? _uow.Transaction : null;

        var clientData = await _uow.Connection.QueryFirstOrDefaultAsync<ClientDataModel>(sql, param, transaction);
        if (clientData is null) return null;

        var client = HydrateClient(clientData);

        const string addressesSql = "SELECT * FROM addresses WHERE client_id = @ClientId AND deleted_at IS NULL;";
        var addressesData = await _uow.Connection.QueryAsync<AddressDataModel>(addressesSql, new { ClientId = client.Id }, transaction);
        foreach (var addressData in addressesData)
        {
            client.AddAddress(HydrateAddress(addressData), Notification.Create());
        }

        const string cardsSql = "SELECT * FROM saved_cards WHERE client_id = @ClientId AND deleted_at IS NULL;";
        var cardsData = await _uow.Connection.QueryAsync<SavedCardDataModel>(cardsSql, new { ClientId = client.Id }, transaction);
        foreach (var cardData in cardsData)
        {
            client.AddSavedCard(cardData.last_four_digits, Enum.Parse<CardBrand>(cardData.brand, true), cardData.gateway_token, DateOnly.FromDateTime(cardData.expiry_date), cardData.nickname);
        }

        return client;
    }
    private static Client HydrateClient(ClientDataModel model)
    {
        var clientRole = Enum.Parse<Role>(model.role, true);
        var clientStatus = model.status switch { "ativo" => ClientStatus.Active, "inativo" => ClientStatus.Inactive, "banido" => ClientStatus.Banned, _ => throw new InvalidOperationException($"Status '{model.status}' do banco de dados não pôde ser mapeado.") };
        return Client.With(model.client_id, model.first_name, model.last_name, model.email, model.email_verified_at, model.phone, model.password_hash, model.cpf, model.date_of_birth.HasValue ? DateOnly.FromDateTime(model.date_of_birth.Value) : null, model.newsletter_opt_in, clientStatus, clientRole, model.failed_login_attempts, model.account_locked_until, model.created_at, model.updated_at, model.deleted_at);
    }
    private static Address HydrateAddress(AddressDataModel model)
    {
        return Address.With(model.address_id, model.client_id, Enum.Parse<AddressType>(model.type, true), model.postal_code, model.street, model.street_number, model.complement, model.neighborhood, model.city, model.state_code, model.country_code, model.is_default, model.created_at, model.updated_at, model.deleted_at);
    }
    private async Task InsertAddresses(IEnumerable<Address> addresses, CancellationToken cancellationToken)
    {
        const string sql = "INSERT INTO addresses (address_id, client_id, type, postal_code, street, street_number, complement, neighborhood, city, state_code, country_code, is_default, created_at, updated_at, version) VALUES (@Id, @ClientId, @TypeString::address_type_enum, @PostalCode, @Street, @StreetNumber, @Complement, @Neighborhood, @City, @StateCode, @CountryCode, @IsDefault, @CreatedAt, @UpdatedAt, 1);";
        foreach (var address in addresses)
        {
            await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, new { address.Id, address.ClientId, TypeString = address.Type.ToString().ToLower(), address.PostalCode, address.Street, address.StreetNumber, address.Complement, address.Neighborhood, address.City, address.StateCode, address.CountryCode, address.IsDefault, address.CreatedAt, address.UpdatedAt }, _uow.Transaction, cancellationToken: cancellationToken));
        }
    }
    private static SavedCard HydrateSavedCard(SavedCardDataModel model)
    {
        return SavedCard.NewCard(model.client_id, model.last_four_digits, Enum.Parse<CardBrand>(model.brand, true), model.gateway_token, DateOnly.FromDateTime(model.expiry_date), model.is_default, model.nickname);
    }
    private Task InsertSavedCards(IEnumerable<SavedCard> cards, CancellationToken cancellationToken) => Task.CompletedTask;
    private Task InsertConsents(IEnumerable<Consent> consents, CancellationToken cancellationToken) => Task.CompletedTask;
}