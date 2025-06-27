using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bcomerce.Application.UseCases.Catalog.Products.ListPublicProducts;

public record ListPublicProductsInput(
    int Page = 1,
    int PageSize = 12,
    string? SearchTerm = null,
    string? CategorySlug = null, // Para filtrar por categoria
    string? BrandSlug = null,    // Para filtrar por marca
    string? SortBy = "name",
    string? SortDirection = "asc"
);