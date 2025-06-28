using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;
using Bcommerce.Domain.Catalog.Products.Repositories;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Products.ListProducts;

public class ListProductsUseCase : IListProductsUseCase
{
    private readonly IProductRepository _productRepository;

    public ListProductsUseCase(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<IEnumerable<ProductOutput>, Notification>> Execute(ListProductsInput input)
    {
        var products = await _productRepository.ListAsync(
            input.Page,
            input.PageSize,
            input.SearchTerm,
            // --- FIX: Add the missing arguments for categoryId and brandId ---
            input.CategoryId, 
            input.BrandId,
            // -----------------------------------------------------------------
            input.SortBy ?? "name",
            input.SortDirection ?? "asc",
            CancellationToken.None
        );
        
        var output = products.Select(product => new ProductOutput(
            product.Id,
            product.BaseSku,
            product.Name,
            product.Slug,
            product.Description,
            product.BasePrice.Amount,
            product.StockQuantity,
            product.IsActive,
            product.CategoryId,
            product.BrandId,
            product.CreatedAt
        ));

        return Result<IEnumerable<ProductOutput>, Notification>.Ok(output);
    }
}