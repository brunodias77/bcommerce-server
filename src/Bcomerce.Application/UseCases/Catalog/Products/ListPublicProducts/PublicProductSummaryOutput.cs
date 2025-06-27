using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bcomerce.Application.UseCases.Catalog.Products.ListPublicProducts;

public record PublicProductSummaryOutput(
    Guid Id,
    string Name,
    string Slug,
    decimal BasePrice,
    decimal? SalePrice,
    string? CoverImageUrl // Apenas a imagem de capa
);
