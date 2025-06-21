using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Customers.Clients.Events;

/// <summary>
/// Evento de domínio que é disparado quando um novo cliente é registrado com sucesso.
/// </summary>
/// <param name="ClientId">O ID do cliente que foi criado.</param>
/// <param name="FirstName">O primeiro nome do cliente.</param>
/// <param name="Email">O e-mail do cliente, usado para comunicação (ex: envio de e-mail de boas-vindas).</param>
public record ClientCreatedEvent(
    Guid ClientId,
    string FirstName,
    string Email
) : DomainEvent;