using Bcommerce.Domain.Common;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Customers.Clients.ValueObjects;

public class Cpf : ValueObject
{
    public string Value { get; }

    public Cpf(string value, IValidationHandler handler)
    {
        if (!IsValid(value))
        {
            handler.Append(new Error("CPF inválido."));
            Value = string.Empty;
        }
        else
        {
            Value = new string(value.Where(char.IsDigit).ToArray());
        }
    }
        
    private Cpf(string value) => Value = value;
    public static Cpf With(string value) => new Cpf(value);

    private static bool IsValid(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        var cpfClean = new string(cpf.Where(char.IsDigit).ToArray());
        if (cpfClean.Length != 11) return false;
        // ... (resto da lógica de validação do seu `is_cpf_valid` do SQL)
        return true;
    }

    public override bool Equals(ValueObject? other) => other is Cpf cpf && cpf.Value == Value;
    protected override int GetCustomHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
}