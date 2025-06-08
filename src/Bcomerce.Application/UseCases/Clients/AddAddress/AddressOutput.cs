using Bcommerce.Domain.Clients;
using Bcommerce.Domain.Clients.enums;

namespace Bcomerce.Application.UseCases.Clients.AddAddress;

public record AddressOutput(
    Guid Id,
    Guid ClientId,
    AddressType Type,
    string PostalCode,
    string Street,
    string Number,
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
            address.Number,
            address.Complement,
            address.Neighborhood,
            address.City,
            address.StateCode,
            address.IsDefault
        );
    }
}