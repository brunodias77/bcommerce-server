using System.Text.RegularExpressions;
using Bcommerce.Domain.Common;
using Bcommerce.Domain.Validation;

namespace Bcommerce.Domain.Customers.Clients.ValueObjects;

public class Email : ValueObject
{
    public string Value { get; }

    public Email(string address, IValidationHandler handler)
    {
        if (!IsValid(address))
        {
            handler.Append(new Error("Endereço de e-mail inválido."));
            Value = string.Empty;
        }
        else
        {
            Value = address;
        }
    }
    private Email(string address) => Value = address;
    public static Email With(string address) => new(address);

    private static bool IsValid(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
    }

    public override bool Equals(ValueObject? other) => other is Email email && email.Value.Equals(Value, StringComparison.OrdinalIgnoreCase);
    protected override int GetCustomHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);
}