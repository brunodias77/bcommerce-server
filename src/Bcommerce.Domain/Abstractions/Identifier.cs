namespace Bcommerce.Domain.Abstractions;

public abstract class Identifier
{
    public abstract Guid Value { get; }

    public override string ToString() => Value.ToString();
}