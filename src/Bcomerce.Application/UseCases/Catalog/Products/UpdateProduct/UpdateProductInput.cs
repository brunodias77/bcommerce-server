namespace Bcomerce.Application.UseCases.Catalog.Products.UpdateProduct;

public record UpdateProductInput(
    Guid ProductId, // <-- ID do produto a ser atualizado
    string Name,
    string? Description,
    decimal BasePrice,
    int StockQuantity,
    bool IsActive,
    Guid CategoryId,
    Guid? BrandId,
    decimal? WeightKg,
    int? HeightCm,
    int? WidthCm,
    int? DepthCm
);