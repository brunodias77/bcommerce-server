using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;
using Bcomerce.Application.UseCases.Catalog.Products.ListPublicProducts;
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

    public async Task<Result<PagedListOutput<ProductOutput>, Notification>> Execute(ListProductsInput input)
    {
        // Passo 1: Buscar os produtos da página atual (isso já estava correto)
        var products = await _productRepository.ListAsync(
            input.Page,
            input.PageSize,
            input.SearchTerm,
            input.CategoryId,
            input.BrandId,
            input.SortBy ?? "name",
            input.SortDirection ?? "asc",
            CancellationToken.None
        );

        // Passo 2: Mapear os produtos para o DTO de saída
        var outputItems = products.Select(product => new ProductOutput(
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
        )).ToList(); // Materializa a lista aqui

        // Passo 3: Buscar o número total de produtos que correspondem aos filtros
        var totalCount = await _productRepository.CountAsync(
            input.SearchTerm,
            input.CategoryId,
            input.BrandId,
            CancellationToken.None
        );
        
        // Passo 4: Construir e retornar o objeto de paginação completo
        var pagedOutput = new PagedListOutput<ProductOutput>(
            input.Page,
            input.PageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)input.PageSize),
            outputItems
        );

        return Result<PagedListOutput<ProductOutput>, Notification>.Ok(pagedOutput);
    }
}