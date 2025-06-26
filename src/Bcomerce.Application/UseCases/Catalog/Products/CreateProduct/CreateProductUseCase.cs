using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Catalog.Brands.Repositories;
using Bcommerce.Domain.Catalog.Categories.Repositories;
using Bcommerce.Domain.Catalog.Products;
using Bcommerce.Domain.Catalog.Products.Repositories;
using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;

public class CreateProductUseCase : ICreateProductUseCase
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _uow;

    public CreateProductUseCase(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IBrandRepository brandRepository,
        IUnitOfWork uow)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _brandRepository = brandRepository;
        _uow = uow;
    }

    public async Task<Result<ProductOutput, Notification>> Execute(CreateProductInput input)
    {
        var notification = Notification.Create();

        // 1. Validações de pré-condições de negócio
        if (await _productRepository.GetByBaseSkuAsync(input.BaseSku, CancellationToken.None) is not null)
        {
            notification.Append(new Error($"Um produto com o SKU '{input.BaseSku}' já existe."));
        }

        if (await _categoryRepository.Get(input.CategoryId, CancellationToken.None) is null)
        {
            notification.Append(new Error($"A categoria com o ID '{input.CategoryId}' não foi encontrada."));
        }
        
        if (input.BrandId.HasValue && await _brandRepository.Get(input.BrandId.Value, CancellationToken.None) is null)
        {
            notification.Append(new Error($"A marca com o ID '{input.BrandId.Value}' não foi encontrada."));
        }
        
        if (notification.HasError())
        {
            return Result<ProductOutput, Notification>.Fail(notification);
        }

        // 2. Criação dos Value Objects e da Entidade de Domínio
        var dimensions = Dimensions.Create(input.WeightKg, input.HeightCm, input.WidthCm, input.DepthCm);
        var basePrice = Money.Create(input.BasePrice);
        
        var product = Product.NewProduct(
            input.BaseSku,
            input.Name,
            input.Description,
            basePrice,
            input.StockQuantity,
            input.CategoryId,
            input.BrandId,
            dimensions,
            notification
        );
        
        if (notification.HasError())
        {
            return Result<ProductOutput, Notification>.Fail(notification);
        }

        // 3. Persistência dentro de uma transação
        await _uow.Begin();
        try
        {
            await _productRepository.Insert(product, CancellationToken.None);
            await _uow.Commit();
        }
        catch (Exception)
        {
            await _uow.Rollback();
            notification.Append(new Error("Ocorreu um erro no banco de dados ao salvar o produto."));
            return Result<ProductOutput, Notification>.Fail(notification);
        }

        // 4. Mapeamento para o DTO de saída e retorno de sucesso
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

        return Result<ProductOutput, Notification>.Ok(output);
    }
}