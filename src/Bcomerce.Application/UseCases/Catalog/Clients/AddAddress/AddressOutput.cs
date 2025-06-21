
using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Customers.Clients.Enums;

namespace Bcomerce.Application.UseCases.Catalog.Clients.AddAddress;

public record AddressOutput(
    Guid Id,
    Guid ClientId,
    AddressType Type,
    string PostalCode,
    string Street,
    string StreetNumber, // Renomeado
    string? Complement,
    string Neighborhood,
    string City,
    string StateCode,
    bool IsDefault
)
{
    public static AddressOutput FromAddress(Address address)
    {
        return new AddressOutput(
            address.Id,
            address.ClientId,
            address.Type,
            address.PostalCode,
            address.Street,
            address.StreetNumber, // Renomeado
            address.Complement,
            address.Neighborhood,
            address.City,
            address.StateCode,
            address.IsDefault
        );
    }
}
