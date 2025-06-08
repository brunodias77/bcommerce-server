using Bcommerce.Domain.Clients.enums;

namespace Bcomerce.Application.UseCases.Clients.UpdateAddress;

public record UpdateAddressPayload(
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