using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Common;
using Bcommerce.Domain.Products.Repositories;
using Bcommerce.Domain.Validations;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.GetProductDetails;

public class GetProductDetailsUseCase : IGetProductDetailsUseCase
{
    private readonly IProductRepository _productRepository;

    public GetProductDetailsUseCase(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    public async Task<Result<ProductDetailsOutput, Notification>> Execute(GetProductDetailsInput input)
    {
        var notification = Notification.Create();
        var product = await _productRepository.GetBySlugWithDetailsAsync(input.Slug, CancellationToken.None);

        if (product is null)
        {
            notification.Append(new Error("Produto não encontrado."));
            return Result<ProductDetailsOutput, Notification>.Fail(notification);
        }

        var output = ProductDetailsOutput.FromProduct(product);
        return Result<ProductDetailsOutput, Notification>.Ok(output);    }
}