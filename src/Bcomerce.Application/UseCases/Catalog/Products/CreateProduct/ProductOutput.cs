namespace Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;

public record ProductOutput(
    Guid Id,
    string BaseSku,
    string Name,
    string Slug,
    string? Description,
    decimal BasePrice,
    int StockQuantity,
    bool IsActive,
    Guid CategoryId,
    Guid? BrandId,
    DateTime CreatedAt
);