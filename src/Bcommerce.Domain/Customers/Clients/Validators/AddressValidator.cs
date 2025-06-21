using Bcommerce.Domain.Customers.Clients.Entities;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Customers.Clients.Validators;

public class AddressValidator : Validator
{
    private readonly Address _address;

    public AddressValidator(Address address, IValidationHandler handler) : base(handler)
    {
        _address = address;
    }

    public override void Validate()
    {
        if (_address.ClientId == Guid.Empty)
            ValidationHandler.Append(new Error("'ClientId' do endereço é obrigatório."));
            
        if (string.IsNullOrWhiteSpace(_address.PostalCode) || _address.PostalCode.Length != 8)
            ValidationHandler.Append(new Error("'PostalCode' deve ter 8 dígitos."));

        if (string.IsNullOrWhiteSpace(_address.Street))
            ValidationHandler.Append(new Error("'Street' é obrigatório."));

        // ATUALIZADO AQUI
        if (string.IsNullOrWhiteSpace(_address.StreetNumber))
            ValidationHandler.Append(new Error("'StreetNumber' (número) é obrigatório."));

        if (string.IsNullOrWhiteSpace(_address.Neighborhood))
            ValidationHandler.Append(new Error("'Neighborhood' (bairro) é obrigatório."));
                
        if (string.IsNullOrWhiteSpace(_address.City))
            ValidationHandler.Append(new Error("'City' é obrigatório."));

        if (string.IsNullOrWhiteSpace(_address.StateCode) || _address.StateCode.Length != 2)
            ValidationHandler.Append(new Error("'StateCode' (UF) deve ter 2 caracteres."));
    }
}