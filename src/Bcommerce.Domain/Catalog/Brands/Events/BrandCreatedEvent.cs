using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Catalog.Brands.Events;

public record BrandCreatedEvent(Guid BrandId, string BrandName) : DomainEvent;
