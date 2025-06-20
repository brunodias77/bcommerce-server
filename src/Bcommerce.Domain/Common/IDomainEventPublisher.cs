using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Common;

public interface IDomainEventPublisher
{
    Task PublishAsync<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken) where TDomainEvent : DomainEvent;
}