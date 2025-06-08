using Bcommerce.Domain.Clients;
using Bcommerce.Domain.Clients.enums;
using Bcommerce.Domain.Clients.Repositories;
using Bcommerce.Infrastructure.Data.Models;
using Dapper;

namespace Bcommerce.Infrastructure.Data.Repositories;

public class AddressRepository : IAddressRepository
{
    
    private readonly IUnitOfWork _uow;

    public AddressRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }
    
public async Task AddAsync(Address address, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO address (address_id, client_id, type, postal_code, street, number, complement, neighborhood, city, state_code, country_code, is_default, created_at, updated_at, deleted_at)
            VALUES (@Id, @ClientId, @TypeString::address_type_enum, @PostalCode, @Street, @Number, @Complement, @Neighborhood, @City, @StateCode, @CountryCode, @IsDefault, @CreatedAt, @UpdatedAt, @DeletedAt);
        ";

        var parameters = new
        {
            address.Id,
            address.ClientId,
            TypeString = address.Type.ToString().ToLower(), // 'shipping' ou 'billing'
            address.PostalCode,
            address.Street,
            address.Number,
            address.Complement,
            address.Neighborhood,
            address.City,
            address.StateCode,
            address.CountryCode,
            address.IsDefault,
            address.CreatedAt,
            address.UpdatedAt,
            address.DeletedAt
        };

        await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, parameters, _uow.Transaction, cancellationToken: cancellationToken));
    }

    public async Task<Address?> GetByIdAsync(Guid addressId, CancellationToken cancellationToken)
    {
        const string sql = "SELECT * FROM address WHERE address_id = @AddressId AND deleted_at IS NULL;";
        var model = await _uow.Connection.QuerySingleOrDefaultAsync<AddressDataModel>(
            sql, 
            new { AddressId = addressId },
            transaction: _uow.HasActiveTransaction ? _uow.Transaction : null
        );

        return model is null ? null : Hydrate(model);
    }

    public async Task<IEnumerable<Address>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken)
    {
        const string sql = "SELECT * FROM address WHERE client_id = @ClientId AND deleted_at IS NULL ORDER BY is_default DESC, created_at ASC;";
        var models = await _uow.Connection.QueryAsync<AddressDataModel>(
            sql,
            new { ClientId = clientId },
            transaction: _uow.HasActiveTransaction ? _uow.Transaction : null
        );

        return models.Select(Hydrate);
    }

    public async Task UpdateAsync(Address address, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE address SET
                type = @TypeString::address_type_enum,
                postal_code = @PostalCode,
                street = @Street,
                number = @Number,
                complement = @Complement,
                neighborhood = @Neighborhood,
                city = @City,
                state_code = @StateCode,
                is_default = @IsDefault,
                updated_at = @UpdatedAt,
                deleted_at = @DeletedAt
            WHERE address_id = @Id;
        ";

        var parameters = new
        {
            address.Id,
            TypeString = address.Type.ToString().ToLower(),
            address.PostalCode,
            address.Street,
            address.Number,
            address.Complement,
            address.Neighborhood,
            address.City,
            address.StateCode,
            address.IsDefault,
            address.UpdatedAt,
            address.DeletedAt
        };

        await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, parameters, _uow.Transaction, cancellationToken: cancellationToken));
    }

    private static Address Hydrate(AddressDataModel model)
    {
        return Address.With(
            model.address_id,
            model.client_id,
            Enum.Parse<AddressType>(model.type, true),
            model.postal_code,
            model.street,
            model.number,
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
}