namespace Bcommerce.Domain.Abstractions;

public interface IDomainEventPublisher
{
    Task PublishAsync<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken) where TDomainEvent : DomainEvent;
}
