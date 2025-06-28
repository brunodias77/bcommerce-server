using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bcomerce.Application.UseCases.Catalog.Products.GetPublicProduct;

public record ProductVariantOutput(Guid VariantId, string Sku, Guid? ColorId, Guid? SizeId, int StockQuantity, decimal AdditionalPrice, string? ImageUrl);

