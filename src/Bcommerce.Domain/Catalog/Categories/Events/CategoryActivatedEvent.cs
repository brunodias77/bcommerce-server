using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Catalog.Categories.Events;

public record CategoryActivatedEvent(Guid CategoryId) : DomainEvent;
