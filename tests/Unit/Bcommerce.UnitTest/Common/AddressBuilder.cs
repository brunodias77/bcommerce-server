using Bcommerce.Domain.Clients;
using Bcommerce.Domain.Clients.enums;
using Bcommerce.Domain.Validations.Handlers;
using Bogus;

namespace Bcommerce.UnitTest.Common;

public class AddressBuilder
{
    private readonly Faker _faker = FakerGenerator.Faker;

    // Campos para armazenar os valores do objeto a ser construído
    private Guid _id = Guid.NewGuid(); // <<< ADICIONE um ID padrão
    private Guid _clientId = Guid.NewGuid();
    private AddressType _type = AddressType.Shipping;
    private string _postalCode;
    private string _street;
    private string _number;
    private string? _complement;
    private string _neighborhood;
    private string _city;
    private string _stateCode;
    private bool _isDefault = false;

    private AddressBuilder()
    {
        // Inicializa com dados falsos e válidos
        _postalCode = _faker.Address.ZipCode("########");
        _street = _faker.Address.StreetName();
        _number = _faker.Address.BuildingNumber();
        _complement = _faker.Address.SecondaryAddress();
        _neighborhood = _faker.Address.StreetSuffix();
        _city = _faker.Address.City();
        _stateCode = _faker.Address.StateAbbr();
    }

    public static AddressBuilder New() => new();
    // <<< ADICIONE ESTE MÉTODO PARA CONTROLAR O ID >>>
    public AddressBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }
    public AddressBuilder WithClientId(Guid clientId)
    {
        _clientId = clientId;
        return this;
    }

    public Address Build()
    {
        // Chamamos o método de fábrica da entidade para criar o objeto
        var address = Address.NewAddress(
            _clientId, _type, _postalCode, _street, _number, _complement,
            _neighborhood, _city, _stateCode, _isDefault,
            Notification.Create()
        );
        
        // Atribuímos o ID manualmente. Isso requer que o setter do Id na classe
        // base 'Entity' seja acessível (geralmente 'protected set').
        // Vamos precisar ajustar a entidade para permitir isso.
        
        // Esta é a forma mais robusta, vamos ajustar a entidade para isso.
        return Address.With(
            _id, _clientId, _type, _postalCode, _street, _number, _complement,
            _neighborhood, _city, _stateCode, "BR", _isDefault,
            DateTime.UtcNow, DateTime.UtcNow, null
        );
    }
}