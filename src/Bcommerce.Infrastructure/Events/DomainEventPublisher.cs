using Bcommerce.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Bcommerce.Infrastructure.Events;

public class DomainEventPublisher : IDomainEventPublisher
{
    public DomainEventPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private readonly IServiceProvider _serviceProvider;

    public async Task PublishAsync<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken) where TDomainEvent : DomainEvent
    {
        // Usamos o ServiceProvider para solicitar TODOS os serviços
        // que implementam o handler para o tipo específico do nosso evento.
        var handlers = _serviceProvider.GetServices<IDomainEventHandler<TDomainEvent>>();

        // Dispara todos os handlers encontrados em paralelo
        await Task.WhenAll(handlers.Select(handler =>
            handler.HandleAsync(domainEvent, cancellationToken)));    }
}