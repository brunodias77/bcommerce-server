using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Catalog.Brands.Events;

public record BrandUpdatedEvent(Guid BrandId) : DomainEvent;
