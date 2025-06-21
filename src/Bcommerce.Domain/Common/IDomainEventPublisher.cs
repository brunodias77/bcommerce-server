using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Common;
/// <summary>
/// Define a interface para um publicador de eventos de domínio.
/// É responsável por despachar eventos para seus respectivos handlers.
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Publica um evento de domínio de forma assíncrona.
    /// </summary>
    /// <typeparam name="TDomainEvent">O tipo do evento a ser publicado.</typeparam>
    /// <param name="domainEvent">A instância do evento a ser publicada.</param>
    /// <param name="cancellationToken">Um token para observar solicitações de cancelamento.</param>

    Task PublishAsync<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken) where TDomainEvent : DomainEvent;
}