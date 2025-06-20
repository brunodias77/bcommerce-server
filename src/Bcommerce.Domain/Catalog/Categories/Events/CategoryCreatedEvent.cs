using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Catalog.Categories.Events;

public record CategoryCreatedEvent(Guid CategoryId, string CategoryName) : DomainEvent;
