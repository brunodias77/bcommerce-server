using Bcommerce.Domain.Abstractions;

namespace Bcommerce.Domain.Clients.Events;

public sealed record ClientCreatedEvent(Guid ClientId, string Email, string FirstName) : DomainEvent;