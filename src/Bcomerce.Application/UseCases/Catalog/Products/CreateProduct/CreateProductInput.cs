namespace Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;

public record CreateProductInput(
    string BaseSku,
    string Name,
    string? Description,
    decimal BasePrice,
    int StockQuantity,
    Guid CategoryId,
    Guid? BrandId,
    // Dados para o Value Object 'Dimensions'
    decimal? WeightKg,
    int? HeightCm,
    int? WidthCm,
    int? DepthCm
);