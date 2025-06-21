using System.Text.Json.Serialization;
using Bcommerce.Domain.Common;
using Bcommerce.Domain.Exceptions;

namespace Bcommerce.Domain.Catalog.Products.ValueObjects;

[Serializable]
public class Money : ValueObject
{
    [JsonPropertyName("amount")]
    public decimal Amount { get; }
    
    [JsonPropertyName("currency")]
    public string Currency { get; }

    [JsonConstructor]
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "BRL")
    {
        DomainException.ThrowWhen(amount < 0, "O valor monetário não pode ser negativo.");
        DomainException.ThrowWhen(string.IsNullOrWhiteSpace(currency), "A moeda não pode ser nula ou vazia.");

        return new Money(amount, currency);
    }

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Não é possível somar valores de moedas diferentes.");
        return Create(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Não é possível subtrair valores de moedas diferentes.");
        return Create(a.Amount - b.Amount, a.Currency);
    }

    public override bool Equals(ValueObject? other)
    {
        if (other is not Money money)
            return false;

        return Amount == money.Amount && Currency == money.Currency;
    }

    protected override int GetCustomHashCode()
    {
        return HashCode.Combine(Amount, Currency);
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}