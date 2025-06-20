using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Customers.Clients.Enums;
using Bcommerce.Domain.Customers.Clients.Repositories;
using Bcommerce.Domain.Customers.Consents;
using Bcommerce.Infrastructure.Data.Mappers;
using Bcommerce.Infrastructure.Data.Models;
using Dapper;

namespace Bcommerce.Infrastructure.Data.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly IUnitOfWork _uow;

    public ClientRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task Insert(Client aggregate, CancellationToken cancellationToken)
    {
        const string clientSql = @"
            INSERT INTO clients (client_id, first_name, last_name, email, email_verified_at, phone, password_hash, cpf, date_of_birth, newsletter_opt_in, status, created_at, updated_at, version)
            VALUES (@Id, @FirstName, @LastName, @EmailValue, @EmailVerified, @PhoneNumber, @PasswordHash, @CpfValue, @DateOfBirth, @NewsletterOptIn, @StatusString, @CreatedAt, @UpdatedAt, 1);
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
            aggregate.UpdatedAt,
            aggregate.DeletedAt
        }, _uow.Transaction, cancellationToken: cancellationToken));
    }
        
    public async Task<Client?> Get(Guid id, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT 
                c.*,
                a.address_id as Id, a.*,
                sc.saved_card_id as Id, sc.*
            FROM clients c
            LEFT JOIN addresses a ON c.client_id = a.client_id AND a.deleted_at IS NULL
            LEFT JOIN saved_cards sc ON c.client_id = sc.client_id AND sc.deleted_at IS NULL
            WHERE c.client_id = @Id AND c.deleted_at IS NULL;
        ";
        
        return await QueryAndHydrateClient(sql, new { Id = id });
    }

    public async Task<Client?> GetByEmail(string email, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT 
                c.*,
                a.address_id as Id, a.*,
                sc.saved_card_id as Id, sc.*
            FROM clients c
            LEFT JOIN addresses a ON c.client_id = a.client_id AND a.deleted_at IS NULL
            LEFT JOIN saved_cards sc ON c.client_id = sc.client_id AND sc.deleted_at IS NULL
            WHERE c.email = @Email AND c.deleted_at IS NULL;
        ";

        return await QueryAndHydrateClient(sql, new { Email = email });
    }

    public async Task Delete(Client aggregate, CancellationToken cancellationToken)
    {
        const string sql = "UPDATE clients SET deleted_at = @Now, status = 'inativo' WHERE client_id = @Id;";
        await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, new { aggregate.Id, Now = DateTime.UtcNow }, _uow.Transaction, cancellationToken: cancellationToken));
    }

    private async Task<Client?> QueryAndHydrateClient(string sql, object param)
    {
        var clientDict = new Dictionary<Guid, Client>();

        await _uow.Connection.QueryAsync<ClientDataModel, AddressDataModel, SavedCardDataModel, Client>(
            sql,
            (clientData, addressData, cardData) =>
            {
                if (!clientDict.TryGetValue(clientData.client_id, out var client))
                {
                    client = HydrateClient(clientData);
                    clientDict.Add(client.Id, client);
                }

                // **CORREÇÃO APLICADA AQUI**
                // Só hidrata o endereço se a chave primária dele não for nula/vazia.
                if (addressData != null && addressData.address_id != Guid.Empty && client.Addresses.All(a => a.Id != addressData.address_id))
                {
                    var address = HydrateAddress(addressData);
                    client.AddAddress(address, Bcommerce.Domain.Validation.Handlers.Notification.Create());
                }

                // **CORREÇÃO APLICADA AQUI (para consistência)**
                // A mesma lógica se aplica aos cartões salvos.
                if (cardData != null && cardData.saved_card_id != Guid.Empty && client.SavedCards.All(sc => sc.Id != cardData.saved_card_id))
                {
                    var savedCard = HydrateSavedCard(cardData);
                    client.AddSavedCard(savedCard.LastFourDigits, savedCard.Brand, savedCard.GatewayToken, savedCard.ExpiryDate, savedCard.Nickname);
                }
                
                return client;
            },
            param,
            transaction: _uow.HasActiveTransaction ? _uow.Transaction : null,
            splitOn: "Id,Id"
        );
        return clientDict.Values.FirstOrDefault();
    }

    private static Client HydrateClient(ClientDataModel model)
    {
        // **CORREÇÃO APLICADA AQUI**
        // Mapeamento manual da string do banco para o enum.
        var clientStatus = model.status switch
        {
            "ativo" => ClientStatus.Active,
            "inativo" => ClientStatus.Inactive,
            "banido" => ClientStatus.Banned,
            _ => throw new InvalidOperationException($"Status '{model.status}' do banco de dados não pôde ser mapeado.")
        };

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
            clientStatus, // Usando o status mapeado
            model.created_at,
            model.updated_at,
            model.deleted_at
        );
    }

    private static Address HydrateAddress(AddressDataModel model)
    {
        return Address.With(
            model.address_id,
            model.client_id,
            Enum.Parse<AddressType>(model.type, true),
            model.postal_code,
            model.street, 
            model.street_number,
            model.complement,
            model.neighborhood,
            model.city,
            model.state_code, 
            model.country_code,
            model.is_default,
            model.created_at,
            model.updated_at,
            model.deleted_at
        );
    }
    
    private async Task InsertAddresses(IEnumerable<Address> addresses, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO addresses (address_id, client_id, type, postal_code, street, street_number, complement, neighborhood, city, state_code, country_code, is_default, created_at, updated_at, version)
            VALUES (@Id, @ClientId, @TypeString::address_type_enum, @PostalCode, @Street, @StreetNumber, @Complement, @Neighborhood, @City, @StateCode, @CountryCode, @IsDefault, @CreatedAt, @UpdatedAt, 1);
        ";
        foreach (var address in addresses)
        {
            await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, new {
                address.Id,
                address.ClientId,
                TypeString = address.Type.ToString().ToLower(), 
                address.PostalCode,
                address.Street, 
                address.StreetNumber,
                address.Complement,
                address.Neighborhood,
                address.City,
                address.StateCode, 
                address.CountryCode,
                address.IsDefault,
                address.CreatedAt,
                address.UpdatedAt
            }, _uow.Transaction, cancellationToken: cancellationToken));
        }
    }
    
    private static SavedCard HydrateSavedCard(SavedCardDataModel model)
    {
        return SavedCard.NewCard(
            model.client_id,
            model.last_four_digits,
            Enum.Parse<CardBrand>(model.brand, true),
            model.gateway_token,
            DateOnly.FromDateTime(model.expiry_date),
            model.is_default,
            model.nickname
        );
    }

    private Task InsertSavedCards(IEnumerable<SavedCard> cards, CancellationToken cancellationToken) => Task.CompletedTask;
    private Task InsertConsents(IEnumerable<Consent> consents, CancellationToken cancellationToken) => Task.CompletedTask;
}




// using Bcommerce.Domain.Customers.Clients;
// using Bcommerce.Domain.Customers.Clients.Entities;
// using Bcommerce.Domain.Customers.Clients.Enums;
// using Bcommerce.Domain.Customers.Clients.Repositories;
// using Bcommerce.Domain.Customers.Consents;
// using Bcommerce.Infrastructure.Data.Mappers;
// using Bcommerce.Infrastructure.Data.Models;
// using Dapper;
//
// namespace Bcommerce.Infrastructure.Data.Repositories;
//
// public class ClientRepository : IClientRepository
// {
//     private readonly IUnitOfWork _uow;
//
//     public ClientRepository(IUnitOfWork uow)
//     {
//         _uow = uow;
//     }
//
//     public async Task Insert(Client aggregate, CancellationToken cancellationToken)
//     {
//         const string clientSql = @"
//             INSERT INTO clients (client_id, first_name, last_name, email, email_verified_at, phone, password_hash, cpf, date_of_birth, newsletter_opt_in, status, created_at, updated_at, version)
//             VALUES (@Id, @FirstName, @LastName, @EmailValue, @EmailVerified, @PhoneNumber, @PasswordHash, @CpfValue, @DateOfBirth, @NewsletterOptIn, @StatusString, @CreatedAt, @UpdatedAt, 1);
//         ";
//
//         await _uow.Connection.ExecuteAsync(new CommandDefinition(clientSql, new
//         {
//             aggregate.Id,
//             aggregate.FirstName,
//             aggregate.LastName,
//             EmailValue = aggregate.Email.Value,
//             aggregate.EmailVerified,
//             aggregate.PhoneNumber,
//             aggregate.PasswordHash,
//             CpfValue = aggregate.Cpf?.Value,
//             // CORREÇÃO: Converte DateOnly? para DateTime? para ser compatível com o Dapper.
//             // TimeOnly.MinValue representa o início do dia (00:00:00).
//             DateOfBirth = aggregate.DateOfBirth?.ToDateTime(TimeOnly.MinValue),
//             aggregate.NewsletterOptIn,
//             StatusString = aggregate.Status.ToDbString(),
//             aggregate.CreatedAt,
//             aggregate.UpdatedAt
//         }, _uow.Transaction, cancellationToken: cancellationToken));
//
//         await InsertAddresses(aggregate.Addresses, cancellationToken);
//         await InsertSavedCards(aggregate.SavedCards, cancellationToken);
//         await InsertConsents(aggregate.Consents, cancellationToken);
//     }
//
//     public async Task Update(Client aggregate, CancellationToken cancellationToken)
//     {
//         const string clientSql = @"
//             UPDATE clients SET
//                 first_name = @FirstName,
//                 last_name = @LastName,
//                 email = @EmailValue,
//                 email_verified_at = @EmailVerified,
//                 phone = @PhoneNumber,
//                 password_hash = @PasswordHash,
//                 cpf = @CpfValue,
//                 date_of_birth = @DateOfBirth,
//                 newsletter_opt_in = @NewsletterOptIn,
//                 status = @StatusString,
//                 updated_at = @UpdatedAt,
//                 deleted_at = @DeletedAt,
//                 version = version + 1
//             WHERE client_id = @Id;
//         ";
//
//         await _uow.Connection.ExecuteAsync(new CommandDefinition(clientSql, new
//         {
//             aggregate.Id,
//             aggregate.FirstName,
//             aggregate.LastName,
//             EmailValue = aggregate.Email.Value,
//             aggregate.EmailVerified,
//             aggregate.PhoneNumber,
//             aggregate.PasswordHash,
//             CpfValue = aggregate.Cpf?.Value,
//             // CORREÇÃO: Converte DateOnly? para DateTime? também no update.
//             DateOfBirth = aggregate.DateOfBirth?.ToDateTime(TimeOnly.MinValue),
//             aggregate.NewsletterOptIn,
//             StatusString = aggregate.Status.ToDbString(),
//             aggregate.UpdatedAt,
//             aggregate.DeletedAt
//         }, _uow.Transaction, cancellationToken: cancellationToken));
//     }
//         
//     // O restante dos métodos (Get, Delete, Hydrate, etc.) permanece o mesmo.
//     public async Task<Client?> Get(Guid id, CancellationToken cancellationToken)
//     {
//         const string sql = @"
//             SELECT 
//                 c.*,
//                 a.address_id as Id, a.*,
//                 sc.saved_card_id as Id, sc.*
//             FROM clients c
//             LEFT JOIN addresses a ON c.client_id = a.client_id AND a.deleted_at IS NULL
//             LEFT JOIN saved_cards sc ON c.client_id = sc.client_id AND sc.deleted_at IS NULL
//             WHERE c.client_id = @Id AND c.deleted_at IS NULL;
//         ";
//         
//         return await QueryAndHydrateClient(sql, new { Id = id });
//     }
//
//     public async Task<Client?> GetByEmail(string email, CancellationToken cancellationToken)
//     {
//         const string sql = @"
//             SELECT 
//                 c.*,
//                 a.address_id as Id, a.*,
//                 sc.saved_card_id as Id, sc.*
//             FROM clients c
//             LEFT JOIN addresses a ON c.client_id = a.client_id AND a.deleted_at IS NULL
//             LEFT JOIN saved_cards sc ON c.client_id = sc.client_id AND sc.deleted_at IS NULL
//             WHERE c.email = @Email AND c.deleted_at IS NULL;
//         ";
//
//         return await QueryAndHydrateClient(sql, new { Email = email });
//     }
//
//     public async Task Delete(Client aggregate, CancellationToken cancellationToken)
//     {
//         const string sql = "UPDATE clients SET deleted_at = @Now, status = 'inativo' WHERE client_id = @Id;";
//         await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, new { aggregate.Id, Now = DateTime.UtcNow }, _uow.Transaction, cancellationToken: cancellationToken));
//     }
//
//     private async Task<Client?> QueryAndHydrateClient(string sql, object param)
//     {
//         var clientDict = new Dictionary<Guid, Client>();
//
//         await _uow.Connection.QueryAsync<ClientDataModel, AddressDataModel, SavedCardDataModel, bool>(
//             sql,
//             (clientData, addressData, cardData) =>
//             {
//                 if (!clientDict.TryGetValue(clientData.client_id, out var client))
//                 {
//                     client = HydrateClient(clientData);
//                     clientDict.Add(client.Id, client);
//                 }
//
//                 if (addressData != null && client.Addresses.All(a => a.Id != addressData.address_id))
//                 {
//                     var address = HydrateAddress(addressData);
//                     client.AddAddress(address, Bcommerce.Domain.Validation.Handlers.Notification.Create());
//                 }
//
//                 if (cardData != null && client.SavedCards.All(sc => sc.Id != cardData.saved_card_id))
//                 {
//                     var savedCard = HydrateSavedCard(cardData);
//                     client.AddSavedCard(savedCard.LastFourDigits, savedCard.Brand, savedCard.GatewayToken, savedCard.ExpiryDate, savedCard.Nickname);
//                 }
//                 
//                 return true;
//             },
//             param,
//             transaction: _uow.HasActiveTransaction ? _uow.Transaction : null,
//             splitOn: "Id,Id"
//         );
//         return clientDict.Values.FirstOrDefault();
//     }
//
//     private static Client HydrateClient(ClientDataModel model)
//     {
//         // Mapeamento manual da string do banco para o enum
//         var clientStatus = model.status switch
//         {
//             "ativo" => ClientStatus.Active,
//             "inativo" => ClientStatus.Inactive,
//             "banido" => ClientStatus.Banned,
//             // Um valor padrão ou exceção para casos inesperados
//             _ => throw new InvalidOperationException($"Status '{model.status}' do banco de dados não pôde ser mapeado.")
//         };
//
//         return Client.With(
//             model.client_id,
//             model.first_name,
//             model.last_name,
//             model.email,
//             model.email_verified_at,
//             model.phone,
//             model.password_hash,
//             model.cpf,
//             model.date_of_birth.HasValue ? DateOnly.FromDateTime(model.date_of_birth.Value) : null,
//             model.newsletter_opt_in,
//             clientStatus, // <- VARIÁVEL COM O STATUS CORRETO AQUI
//             model.created_at,
//             model.updated_at,
//             model.deleted_at
//         );
//     }
//
//     private static Address HydrateAddress(AddressDataModel model)
//     {
//         return Address.With(
//             model.address_id, model.client_id,
//             Enum.Parse<AddressType>(model.type, true),
//             model.postal_code, model.street, 
//             model.street_number,
//             model.complement,
//             model.neighborhood, model.city, model.state_code, 
//             model.country_code,
//             model.is_default, model.created_at, model.updated_at, model.deleted_at
//         );
//     }
//     
//     private async Task InsertAddresses(IEnumerable<Address> addresses, CancellationToken cancellationToken)
//     {
//         const string sql = @"
//             INSERT INTO addresses (address_id, client_id, type, postal_code, street, street_number, complement, neighborhood, city, state_code, country_code, is_default, created_at, updated_at, version)
//             VALUES (@Id, @ClientId, @TypeString::address_type_enum, @PostalCode, @Street, @StreetNumber, @Complement, @Neighborhood, @City, @StateCode, @CountryCode, @IsDefault, @CreatedAt, @UpdatedAt, 1);
//         ";
//         foreach (var address in addresses)
//         {
//             await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, new {
//                 address.Id, address.ClientId, TypeString = address.Type.ToString().ToLower(), 
//                 address.PostalCode, address.Street, 
//                 address.StreetNumber,
//                 address.Complement, address.Neighborhood, address.City, address.StateCode, 
//                 address.CountryCode,
//                 address.IsDefault, address.CreatedAt, address.UpdatedAt
//             }, _uow.Transaction, cancellationToken: cancellationToken));
//         }
//     }
//     
//     private static SavedCard HydrateSavedCard(SavedCardDataModel model)
//     {
//         return SavedCard.NewCard(
//             model.client_id, model.last_four_digits,
//             Enum.Parse<CardBrand>(model.brand, true),
//             model.gateway_token, DateOnly.FromDateTime(model.expiry_date),
//             model.is_default, model.nickname
//         );
//     }
//
//     private Task InsertSavedCards(IEnumerable<SavedCard> cards, CancellationToken cancellationToken) => Task.CompletedTask;
//     private Task InsertConsents(IEnumerable<Consent> consents, CancellationToken cancellationToken) => Task.CompletedTask;
// }
//
