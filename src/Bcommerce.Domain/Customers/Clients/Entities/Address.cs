using Bcommerce.Domain.Common;
using Bcommerce.Domain.Customers.Clients.Enums;
using Bcommerce.Domain.Customers.Clients.Validators;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Customers.Clients.Entities;

public class Address : Entity
{
        public Guid ClientId { get; private set; }
        public AddressType Type { get; private set; }
        public string PostalCode { get; private set; }
        public string Street { get; private set; }
        public string Number { get; private set; }
        public string? Complement { get; private set; }
        public string Neighborhood { get; private set; }
        public string City { get; private set; }
        public string StateCode { get; private set; }
        public string CountryCode { get; private set; }
        public bool IsDefault { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        
        private Address() {}

        public static Address NewAddress(
            Guid clientId, AddressType type, string postalCode, string street, string number,
            string? complement, string neighborhood, string city, string stateCode, bool isDefault,
            IValidationHandler handler)
        {
            var address = new Address {
                ClientId = clientId, Type = type, PostalCode = postalCode, Street = street,
                Number = number, Complement = complement, Neighborhood = neighborhood, City = city,
                StateCode = stateCode, CountryCode = "BR", IsDefault = isDefault,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            address.Validate(handler);
            return address;
        }

        public static Address With(
             Guid id, Guid clientId, AddressType type, string postalCode, string street, string number,
             string? complement, string neighborhood, string city, string stateCode, string countryCode,
             bool isDefault, DateTime createdAt, DateTime updatedAt, DateTime? deletedAt)
        {
            var address = new Address {
                Id = id, ClientId = clientId, Type = type, PostalCode = postalCode, Street = street,
                Number = number, Complement = complement, Neighborhood = neighborhood, City = city,
                StateCode = stateCode, CountryCode = countryCode, IsDefault = isDefault,
                CreatedAt = createdAt, UpdatedAt = updatedAt, DeletedAt = deletedAt
            };
            return address;
        }
        
        public void Update(AddressType type, string postalCode, string street, string number,
            string? complement, string neighborhood, string city, string stateCode, bool isDefault,
            IValidationHandler handler)
        {
            Type = type; PostalCode = postalCode; Street = street; Number = number;
            Complement = complement; Neighborhood = neighborhood; City = city; StateCode = stateCode;
            IsDefault = isDefault; UpdatedAt = DateTime.UtcNow;
            
            Validate(handler);
        }
        
        public void SoftDelete()
        {
            if (DeletedAt.HasValue) return;
            DeletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        internal void SetDefault(bool isDefault)
        {
            IsDefault = isDefault;
            UpdatedAt = DateTime.UtcNow;
        }

        public override void Validate(IValidationHandler handler)
        {
            new AddressValidator(this, handler).Validate();
        }
}

