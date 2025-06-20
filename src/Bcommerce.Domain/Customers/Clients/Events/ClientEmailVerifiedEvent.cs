using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Customers.Clients.Events;

public record ClientEmailVerifiedEvent(Guid ClientId) : DomainEvent;
