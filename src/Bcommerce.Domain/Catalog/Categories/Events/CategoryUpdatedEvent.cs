using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Catalog.Categories.Events;

public record CategoryUpdatedEvent(Guid CategoryId) : DomainEvent;
