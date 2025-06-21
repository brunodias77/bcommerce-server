using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Common;

/// <summary>
/// Define o contrato para um manipulador de evento de domínio.
/// Cada manipulador trata um tipo específico de evento.
/// </summary>
/// <typeparam name="TDomainEvent">Tipo do evento de domínio.</typeparam>
public interface IDomainEventHandler<TDomainEvent> where TDomainEvent : DomainEvent
{
    /// <summary>
    /// Manipula a ocorrência de um evento de domínio de forma assíncrona.
    /// </summary>
    /// <param name="domainEvent">A instância do evento de domínio a ser processada.</param>
    /// <param name="cancellationToken">Um token para observar solicitações de cancelamento.</param>
    /// <returns>Uma tarefa que representa a operação de manipulação assíncrona.</returns>
    Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken);
}