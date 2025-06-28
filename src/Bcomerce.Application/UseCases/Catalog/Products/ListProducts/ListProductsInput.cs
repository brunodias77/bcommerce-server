namespace Bcomerce.Application.UseCases.Catalog.Products.ListProducts;

public record ListProductsInput(
    int Page = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    // --- CORREÇÃO: Adicionados parâmetros para filtro por categoria e marca ---
    Guid? CategoryId = null,
    Guid? BrandId = null,
    // ----------------------------------------------------------------------
    string? SortBy = "name",
    string? SortDirection = "asc"
);