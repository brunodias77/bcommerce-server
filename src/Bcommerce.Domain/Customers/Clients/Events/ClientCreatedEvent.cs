using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Customers.Clients.Events;

public record ClientCreatedEvent(
    Guid ClientId,
    string FirstName,
    string Email
) : DomainEvent;