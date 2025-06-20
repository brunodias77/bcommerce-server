using Bcommerce.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bcommerce.Infrastructure.Events;

public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventPublisher> _logger;

    // Injetando ILogger para registrar informações e erros.
    public DomainEventPublisher(IServiceProvider serviceProvider, ILogger<DomainEventPublisher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken) 
        where TDomainEvent : DomainEvent
    {
        _logger.LogInformation("Publicando evento de domínio: {EventName}", typeof(TDomainEvent).Name);

        // MELHORIA: Usar um escopo de serviço para resolver os handlers.
        // Isso garante que qualquer dependência com tempo de vida "Scoped" (como IUnitOfWork)
        // seja resolvida corretamente para esta operação, evitando problemas de concorrência ou de
        // tempo de vida do objeto (ex: "cannot access a disposed object").
        using var scope = _serviceProvider.CreateScope();
        
        var handlers = scope.ServiceProvider.GetServices<IDomainEventHandler<TDomainEvent>>();

        var handlerTasks = handlers.Select(handler =>
        {
            try
            {
                return handler.HandleAsync(domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                // MELHORIA: Tratamento de erro para não quebrar todo o processo
                // se um único handler falhar.
                _logger.LogError(ex, "Erro ao executar o handler {HandlerName} para o evento {EventName}", 
                    handler.GetType().Name, typeof(TDomainEvent).Name);
                
                // Retorna uma tarefa completada para não parar o Task.WhenAll
                return Task.CompletedTask; 
            }
        });

        await Task.WhenAll(handlerTasks);
        
        _logger.LogInformation("Evento de domínio {EventName} publicado com sucesso para {HandlerCount} handlers.", 
            typeof(TDomainEvent).Name, handlers.Count());
    }
}