using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;
using Bcommerce.Domain.Catalog.Products.Repositories;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Products.GetProductById;

public class GetProductByIdUseCase : IGetProductByIdUseCase
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdUseCase(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductOutput, Notification>> Execute(Guid productId)
    {
        var product = await _productRepository.Get(productId, CancellationToken.None);

        if (product is null)
        {
            var notification = Notification.Create().Append(new Error("Produto não encontrado."));
            return Result<ProductOutput, Notification>.Fail(notification);
        }
        
        // Mapeando a entidade para o DTO de saída
        var output = new ProductOutput(
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
        );

        return Result<ProductOutput, Notification>.Ok(output);    }
}