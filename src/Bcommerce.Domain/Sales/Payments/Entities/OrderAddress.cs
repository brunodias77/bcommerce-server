using Bcommerce.Domain.Common;
using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Customers.Clients.Enums;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Sales.Payments.Entities;

public class OrderAddress : Entity
{
    public Guid OrderId { get; private set; }
    public AddressType AddressType { get; private set; }
    public string RecipientName { get; private set; }
    public string PostalCode { get; private set; }
    public string Street { get; private set; }
    public string Number { get; private set; } // Mantido como 'Number' para consistência com a tabela order_addresses
    public string? Complement { get; private set; }
    public string Neighborhood { get; private set; }
    public string City { get; private set; }
    public string StateCode { get; private set; }
    public string CountryCode { get; private set; }
    public string? Phone { get; private set; }

    private OrderAddress() { }

    internal static OrderAddress CreateFrom(Guid orderId, Address sourceAddress, string recipientName, string? phone)
    {
        return new OrderAddress
        {
            OrderId = orderId,
            AddressType = sourceAddress.Type,
            RecipientName = recipientName,
            PostalCode = sourceAddress.PostalCode,
            Street = sourceAddress.Street,
            Number = sourceAddress.StreetNumber, // ATUALIZADO AQUI
            Complement = sourceAddress.Complement,
            Neighborhood = sourceAddress.Neighborhood,
            City = sourceAddress.City,
            StateCode = sourceAddress.StateCode,
            CountryCode = sourceAddress.CountryCode, // ATUALIZADO AQUI
            Phone = phone
        };
    }

    public override void Validate(IValidationHandler handler) { /* Validações se necessário */ }
}