namespace Bcommerce.Domain.Abstractions;

public abstract record DomainEvent
{
    // Use "init" para tornar a propriedade imutável após a criação
    // e inicialize diretamente. Use UtcNow para consistência.
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}