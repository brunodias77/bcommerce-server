namespace Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;

public record CreateProductInput(
    string Name,
    string Slug,
    string? Description,
    decimal BasePrice,
    int StockQuantity, // Estoque inicial para produtos simples (sem variantes)
    Guid CategoryId,
    Guid? BrandId,
    bool IsActive
);