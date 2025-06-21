using Bcommerce.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bcommerce.Infrastructure.Events;

/// <summary>
/// Publicador de eventos de domínio que opera em memória (in-process).
/// Ele resolve e dispara todos os handlers registrados para um determinado evento.
/// </summary>
public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventPublisher> _logger;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="DomainEventPublisher"/>.
    /// </summary>
    /// <param name="serviceProvider">O provedor de serviços para resolver os handlers de eventos.</param>
    /// <param name="logger">O serviço de logging para registrar informações e erros.</param>
    public DomainEventPublisher(IServiceProvider serviceProvider, ILogger<DomainEventPublisher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    
    /// <summary>
    /// Publica um evento de domínio de forma assíncrona para todos os seus handlers registrados.
    /// </summary>
    /// <typeparam name="TDomainEvent">O tipo do evento de domínio a ser publicado.</typeparam>
    /// <param name="domainEvent">A instância do evento a ser publicada.</param>
    /// <param name="cancellationToken">O token para cancelamento da operação.</param>
    public async Task PublishAsync<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken) 
        where TDomainEvent : DomainEvent
    {
        _logger.LogInformation("Publicando evento de domínio: {EventName}", typeof(TDomainEvent).Name);

        // A boa prática aqui é criar um escopo de injeção de dependência para a execução dos handlers.
        // Isso garante que qualquer dependência com tempo de vida 'Scoped' (como IUnitOfWork ou DbContext)
        // seja criada e descartada corretamente para esta operação, evitando problemas de concorrência
        // ou de tempo de vida do objeto (ex: "cannot access a disposed object").
        // CORREÇÃO: Usando 'await using' e 'CreateAsyncScope()'
        await using var scope = _serviceProvider.CreateAsyncScope();

        // Resolve todos os handlers registrados para o tipo de evento específico.
        var handlers = scope.ServiceProvider.GetServices<IDomainEventHandler<TDomainEvent>>();

        // Prepara uma lista de tarefas para executar todos os handlers de forma concorrente.
        var handlerTasks = handlers.Select(handler =>
        {
            // O bloco try-catch é crucial para a resiliência do sistema.
            // Se um único handler falhar, ele não irá impedir a execução dos outros.
            try
            {
                // Inicia a tarefa de manipulação do evento.
                return handler.HandleAsync(domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                // Em caso de exceção, o erro é logado com detalhes sobre qual handler falhou.
                _logger.LogError(ex, "Erro ao executar o handler {HandlerName} para o evento {EventName}", 
                    handler.GetType().Name, typeof(TDomainEvent).Name);
                
                // Retorna uma tarefa já completada para que a falha de um não pare o `Task.WhenAll`.
                return Task.CompletedTask; 
            }
        });
        
        // Aguarda a conclusão de todos os handlers.
        await Task.WhenAll(handlerTasks);
        
        _logger.LogInformation("Evento de domínio {EventName} publicado com sucesso para {HandlerCount} handlers.", 
            typeof(TDomainEvent).Name, handlers.Count());
    }
}