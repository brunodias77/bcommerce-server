namespace Bcommerce.Domain.Common;

public record ProductSearchQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Keyword = null,
    Guid? CategoryId = null,
    Guid? BrandId = null,
    string? SortBy = null // Ex: "price_asc", "price_desc", "name"
);