using Bcommerce.Domain.Products;

namespace Bcomerce.Application.UseCases.Catalog.Common;

public record ProductDetailsOutput(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    decimal BasePrice,
    Guid CategoryId,
    Guid? BrandId,
    IReadOnlyCollection<ProductVariantOutput> Variants)
{
    public static ProductDetailsOutput FromProduct(Product product)
    {
        var variantOutputs = product.Variants
            .Select(ProductVariantOutput.FromVariant)
            .ToList();

        return new ProductDetailsOutput(
            product.Id,
            product.Name,
            product.Slug,
            product.Description,
            product.BasePrice,
            product.CategoryId,
            product.BrandId,
            variantOutputs
        );
    }
}
