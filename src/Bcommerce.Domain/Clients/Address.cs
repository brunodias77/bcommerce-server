using Bcommerce.Domain.Abstractions;
using Bcommerce.Domain.Clients.enums;
using Bcommerce.Domain.Clients.Validators;
using Bcommerce.Domain.Validations;

namespace Bcommerce.Domain.Clients;

public class Address : Entity{
    
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
    
    // Construtor para o Dapper e métodos de fábrica
    private Address() { }
    
    // Método de fábrica para criar um novo endereço
    public static Address NewAddress(
        Guid clientId,
        AddressType type,
        string postalCode,
        string street,
        string number,
        string? complement,
        string neighborhood,
        string city,
        string stateCode,
        bool isDefault,
        IValidationHandler handler)
    {
        var address = new Address
        {
            ClientId = clientId,
            Type = type,
            PostalCode = postalCode,
            Street = street,
            Number = number,
            Complement = complement,
            Neighborhood = neighborhood,
            City = city,
            StateCode = stateCode,
            CountryCode = "BR", // Padrão
            IsDefault = isDefault,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        address.Validate(handler);
        return address;
    }
    
    // Método para "hidratar" a entidade a partir do banco de dados
    public static Address With(
        Guid id, Guid clientId, AddressType type, string postalCode,
        string street, string number, string? complement,
        string neighborhood, string city, string stateCode, string countryCode,
        bool isDefault, DateTime createdAt, DateTime updatedAt, DateTime? deletedAt)
    {
        var address = new Address
        {
            ClientId = clientId,
            Type = type,
            PostalCode = postalCode,
            Street = street,
            Number = number,
            Complement = complement,
            Neighborhood = neighborhood,
            City = city,
            StateCode = stateCode,
            CountryCode = countryCode,
            IsDefault = isDefault,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            DeletedAt = deletedAt
        };
    
        // Atribui o ID específico
        address.Id = id;
    
        return address;
    }


    public void SetAsDefault()
    {
        if (IsDefault) return;
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UnsetDefault()
    {
        if (!IsDefault) return;
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        if (DeletedAt.HasValue) return;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    // Dentro da classe Address

    public void Update(
        AddressType type, string postalCode, string street,
        string number, string? complement, string neighborhood,
        string city, string stateCode, bool isDefault, IValidationHandler handler)
    {
        Type = type;
        PostalCode = postalCode;
        Street = street;
        Number = number;
        Complement = complement;
        Neighborhood = neighborhood;
        City = city;
        StateCode = stateCode;
        IsDefault = isDefault;
        UpdatedAt = DateTime.UtcNow;

        Validate(handler); // Re-valida a entidade com os novos dados
    }
    
    public override void Validate(IValidationHandler handler)
    {
        new AddressValidator(this, handler).Validate();
    }
}