using Bcommerce.Domain.Products;

namespace Bcomerce.Application.UseCases.Catalog.Common;

public record ProductVariantOutput(
    Guid Id,
    string Sku,
    Guid? ColorId,
    Guid? SizeId,
    int StockQuantity,
    decimal AdditionalPrice)
{
    public static ProductVariantOutput FromVariant(ProductVariant variant)
    {
        return new ProductVariantOutput(
            variant.Id,
            variant.Sku,
            variant.ColorId,
            variant.SizeId,
            variant.StockQuantity,
            variant.AdditionalPrice
        );
    }
}