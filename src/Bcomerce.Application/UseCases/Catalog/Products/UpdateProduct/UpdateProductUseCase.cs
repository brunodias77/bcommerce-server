using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;
using Bcommerce.Domain.Catalog.Brands.Repositories;
using Bcommerce.Domain.Catalog.Categories.Repositories;
using Bcommerce.Domain.Catalog.Products.Repositories;
using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcomerce.Application.UseCases.Catalog.Products.UpdateProduct;

public class UpdateProductUseCase : IUpdateProductUseCase
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _uow;

    public UpdateProductUseCase(IProductRepository productRepository, ICategoryRepository categoryRepository, IBrandRepository brandRepository, IUnitOfWork uow)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _brandRepository = brandRepository;
        _uow = uow;
    }

    public async Task<Result<ProductOutput, Notification>> Execute(UpdateProductInput input)
    {
        var notification = Notification.Create();
        var product = await _productRepository.Get(input.ProductId, CancellationToken.None);

        if (product is null)
        {
            notification.Append(new Error("Produto não encontrado."));
            return Result<ProductOutput, Notification>.Fail(notification);
        }
        
        // Valida se as novas referências (categoria/marca) existem
        if (await _categoryRepository.Get(input.CategoryId, CancellationToken.None) is null)
            notification.Append(new Error($"A categoria com o ID '{input.CategoryId}' não foi encontrada."));
        if (input.BrandId.HasValue && await _brandRepository.Get(input.BrandId.Value, CancellationToken.None) is null)
            notification.Append(new Error($"A marca com o ID '{input.BrandId.Value}' não foi encontrada."));

        if (notification.HasError())
            return Result<ProductOutput, Notification>.Fail(notification);
            
        // Atualiza a entidade de domínio
        product.Update(
            input.Name,
            input.Description,
            Money.Create(input.BasePrice),
            input.StockQuantity,
            input.IsActive,
            input.CategoryId,
            input.BrandId,
            Dimensions.Create(input.WeightKg, input.HeightCm, input.WidthCm, input.DepthCm),
            notification
        );
        
        if (notification.HasError())
            return Result<ProductOutput, Notification>.Fail(notification);
            
        // Persiste as alterações
        await _uow.Begin();
        try
        {
            await _productRepository.Update(product, CancellationToken.None);
            await _uow.Commit();
        }
        catch(Exception)
        {
            await _uow.Rollback();
            notification.Append(new Error("Ocorreu um erro no banco de dados ao atualizar o produto."));
            return Result<ProductOutput, Notification>.Fail(notification);
        }

        var output = new ProductOutput(product.Id, product.BaseSku, product.Name, product.Slug, product.Description, product.BasePrice.Amount, product.StockQuantity, product.IsActive, product.CategoryId, product.BrandId, product.CreatedAt);
        return Result<ProductOutput, Notification>.Ok(output);
    }
}