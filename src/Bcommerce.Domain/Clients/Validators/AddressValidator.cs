using Bcommerce.Domain.Validations;


namespace Bcommerce.Domain.Clients.Validators;

public class AddressValidator : Validator
{
    private readonly Address _address;

    public AddressValidator(Address address, IValidationHandler handler) : base(handler)
    {
        _address = address;

    }

    public override void Validate()
    {
        CheckPostalCode();
        CheckStreet();
        CheckNumber();
        CheckNeighborhood();
        CheckCity();
        CheckStateCode();
    }

    private void CheckPostalCode()
    {
        if (string.IsNullOrWhiteSpace(_address.PostalCode) || _address.PostalCode.Length != 8)
        {
            ValidationHandler.Append(new Error("O CEP deve conter 8 dígitos."));
        }
    }

    private void CheckStreet()
    {
        if (string.IsNullOrWhiteSpace(_address.Street) || _address.Street.Length > 150)
        {
            ValidationHandler.Append(new Error("A rua é obrigatória e não pode exceder 150 caracteres."));
        }
    }

    private void CheckNumber()
    {
        if (string.IsNullOrWhiteSpace(_address.Number))
        {
            ValidationHandler.Append(new Error("O número é obrigatório."));
        }
        else if (_address.Number.Length > 20)
        {
            ValidationHandler.Append(new Error("O número não pode exceder 20 caracteres."));
        }
    }

    private void CheckNeighborhood()
    {
        if (string.IsNullOrWhiteSpace(_address.Neighborhood))
        {
            ValidationHandler.Append(new Error("O bairro é obrigatório."));
        }
        else if (_address.Neighborhood.Length > 100)
        {
            ValidationHandler.Append(new Error("O bairro não pode exceder 100 caracteres."));
        }
    }

    private void CheckCity()
    {
        if (string.IsNullOrWhiteSpace(_address.City))
        {
            ValidationHandler.Append(new Error("A cidade é obrigatória."));
        }
        else if (_address.City.Length > 100)
        {
            ValidationHandler.Append(new Error("A cidade não pode exceder 100 caracteres."));
        }
    }

    private void CheckStateCode()
    {
        if (string.IsNullOrWhiteSpace(_address.StateCode) || _address.StateCode.Length != 2)
        {
            ValidationHandler.Append(new Error("O código do estado (UF) deve conter 2 caracteres."));
        }
    }
}