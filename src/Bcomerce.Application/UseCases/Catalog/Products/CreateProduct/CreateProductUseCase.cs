using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Brands.Repositories;
using Bcommerce.Domain.Categories.Repositories;
using Bcommerce.Domain.Products;
using Bcommerce.Domain.Products.Repositories;
using Bcommerce.Domain.Validations;
using Bcommerce.Domain.Validations.Handlers;
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

    public async Task<Result<CreateProductOutput, Notification>> Execute(CreateProductInput input)
    {
        var notification = Notification.Create();
        
        // 1. Validação de pré-condições da aplicação
        var category = await _categoryRepository.Get(input.CategoryId, CancellationToken.None);
        if (category is null)
        {
            notification.Append(new Error("A categoria fornecida não existe."));
            return Result<CreateProductOutput, Notification>.Fail(notification);
        }

        if (input.BrandId.HasValue)
        {
            var brand = await _brandRepository.Get(input.BrandId.Value, CancellationToken.None);
            if (brand is null)
            {
                notification.Append(new Error("A marca fornecida não existe."));
                return Result<CreateProductOutput, Notification>.Fail(notification);
            }
        }
        
        // Verificar se o SKU base já existe
        var skuExists = await _productRepository.GetByBaseSkuAsync(input.BaseSku, CancellationToken.None);
        if (skuExists is not null)
        {
            notification.Append(new Error("O SKU base fornecido já está em uso por outro produto."));
            return Result<CreateProductOutput, Notification>.Fail(notification);
        }

        // Verificar se o slug já existe
        var slugExists = await _productRepository.GetBySlugAsync(input.Slug, CancellationToken.None);
        if (slugExists is not null)
        {
            notification.Append(new Error("O slug fornecido já está em uso por outro produto."));
            return Result<CreateProductOutput, Notification>.Fail(notification);
        }

        // 2. Criação do Aggregate Root (Produto)
        var product = Product.NewProduct(
            input.BaseSku,
            input.Name,
            input.Slug,
            input.BasePrice,
            input.StockQuantity,
            input.CategoryId,
            input.BrandId
        );

        // Definir propriedades adicionais
        product.SetDescription(input.Description);
        
        if (input.SalePrice.HasValue && input.SalePriceStartDate.HasValue && input.SalePriceEndDate.HasValue)
        {
            product.SetSalePrice(input.SalePrice.Value, input.SalePriceStartDate.Value, input.SalePriceEndDate.Value);
        }

        product.SetDimensions(
            weightKg: input.WeightKg,
            heightCm: input.HeightCm,
            widthCm: input.WidthCm,
            depthCm: input.DepthCm
        );

        // Validar o produto
        product.Validate(notification);
        
        if (notification.HasError())
        {
            return Result<CreateProductOutput, Notification>.Fail(notification);
        }

        // 3. Persistência
        await _uow.Begin();
        try
        {
            await _productRepository.Insert(product, CancellationToken.None);
            await _uow.Commit();
        }
        catch (Exception ex)
        {
            await _uow.Rollback();
            // Logar a exceção 'ex'
            notification.Append(new Error("Erro ao salvar o produto no banco de dados."));
            return Result<CreateProductOutput, Notification>.Fail(notification);
        }

        // 4. Mapeamento do Output e Retorno
        var output = new CreateProductOutput(
            product.Id,
            product.Name,
            product.Slug,
            product.CategoryId,
            product.BrandId,
            product.BasePrice,
            product.SalePrice,
            product.IsActive
        );

        return Result<CreateProductOutput, Notification>.Ok(output);
    }
}