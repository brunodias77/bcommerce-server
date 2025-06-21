using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Catalog.Categories.Events;

public record CategoryDeactivatedEvent(Guid CategoryId) : DomainEvent;
