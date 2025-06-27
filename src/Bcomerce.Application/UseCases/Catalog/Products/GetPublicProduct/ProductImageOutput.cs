using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bcomerce.Application.UseCases.Catalog.Products.GetPublicProduct;

public record ProductImageOutput(string ImageUrl, string? AltText, bool IsCover, int SortOrder);

