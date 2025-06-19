namespace Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;

public record CreateProductOutput(
    Guid Id,
    string Name,
    string Slug,
    Guid CategoryId,
    DateTime CreatedAt
);