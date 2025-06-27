using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Catalog.Products.Repositories;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Products.GetPublicProduct;

public class GetPublicProductBySlugUseCase : IGetPublicProductBySlugUseCase
{
    private readonly IProductRepository _productRepository;

    public GetPublicProductBySlugUseCase(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    public async Task<Result<PublicProductOutput, Notification>> Execute(string slug)
    {
        var product = await _productRepository.GetBySlugAsync(slug, CancellationToken.None);

        if (product is null || !product.IsActive)
        {
            var notification = Notification.Create().Append(new Error("Produto não encontrado."));
            return Result<PublicProductOutput, Notification>.Fail(notification);
        }

        // Mapeando a entidade e suas coleções para os DTOs de saída
        var output = new PublicProductOutput(
            product.Id,
            product.BaseSku,
            product.Name,
            product.Slug,
            product.Description,
            product.BasePrice.Amount,
            product.SalePrice?.Amount,
            product.CategoryId,
            product.BrandId,
            product.Images.Select(img => new ProductImageOutput(img.ImageUrl, img.AltText, img.IsCover, img.SortOrder)).ToList(),
            product.Variants.Select(v => new ProductVariantOutput(v.Id, v.Sku, v.ColorId, v.SizeId, v.StockQuantity, v.AdditionalPrice.Amount, v.ImageUrl)).ToList()
        );

        return Result<PublicProductOutput, Notification>.Ok(output);
    }
}
