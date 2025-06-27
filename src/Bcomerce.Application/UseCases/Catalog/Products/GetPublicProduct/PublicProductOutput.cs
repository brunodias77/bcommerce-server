using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bcomerce.Application.UseCases.Catalog.Products.GetPublicProduct;

public record PublicProductOutput(
    Guid Id,
    string BaseSku,
    string Name,
    string Slug,
    string? Description,
    decimal BasePrice,
    decimal? SalePrice,
    // Referências
    Guid CategoryId,
    Guid? BrandId,
    // Coleções
    IReadOnlyCollection<ProductImageOutput> Images,
    IReadOnlyCollection<ProductVariantOutput> Variants
);
