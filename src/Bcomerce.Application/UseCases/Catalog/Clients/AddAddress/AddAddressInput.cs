using Bcommerce.Domain.Customers.Clients.Enums;

namespace Bcomerce.Application.UseCases.Catalog.Clients.AddAddress;

public record AddAddressInput(
    AddressType Type,
    string PostalCode,
    string Street,
    string StreetNumber, // Renomeado
    string? Complement,
    string Neighborhood,
    string City,
    string StateCode,
    bool IsDefault
);
