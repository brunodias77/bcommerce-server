using Bcommerce.Domain.Clients.enums;

namespace Bcomerce.Application.UseCases.Clients.AddAddress;

public record AddAddressInput(
    AddressType Type,
    string PostalCode,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string StateCode,
    bool IsDefault
);