namespace Bcommerce.Domain.Common;

/// <summary>
/// Representa a classe base para todos os eventos de domínio no sistema.
/// Eventos de domínio são registros imutáveis de algo significativo que aconteceu no domínio.
/// </summary>
public abstract record DomainEvent
{
    /// <summary>
    /// A data e hora em UTC em que o evento ocorreu.
    /// </summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}