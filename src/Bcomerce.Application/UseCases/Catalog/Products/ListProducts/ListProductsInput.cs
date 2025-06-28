namespace Bcomerce.Application.UseCases.Catalog.Products.ListProducts;

public record ListProductsInput(
    int Page = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    string? SortBy = "name",
    string? SortDirection = "asc"
);