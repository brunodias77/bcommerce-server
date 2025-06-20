using Bcommerce.Domain.Customers.Clients.Enums;

namespace Bcomerce.Application.UseCases.Clients.UpdateAddress;

public record UpdateAddressPayload(
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