using Bcommerce.Domain.Common;
using Bcommerce.Domain.Products;
using Bcommerce.Domain.Products.Repositories;
using Bcommerce.Infrastructure.Data.Models;
using Dapper;

namespace Bcommerce.Infrastructure.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IUnitOfWork _uow;

    public ProductRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }
    public async Task Insert(Product aggregate, CancellationToken cancellationToken)
    {
        // Nota: Esta implementação inicial insere apenas o produto principal.
        // A inserção de variantes seria feita em uma transação no Use Case,
        // chamando um repositório de variantes ou expandindo este.
        const string sql = @"
            INSERT INTO product (product_id, name, slug, description, base_price, stock_quantity, is_active, category_id, brand_id, created_at, updated_at)
            VALUES (@Id, @Name, @Slug, @Description, @BasePrice, @StockQuantity, @IsActive, @CategoryId, @BrandId, @CreatedAt, @UpdatedAt);
        ";
        await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, aggregate, _uow.Transaction, cancellationToken: cancellationToken));
    }

    public Task<Product> Get(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Delete(Product aggregate, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Update(Product aggregate, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Product?> GetBySlugWithDetailsAsync(string slug, CancellationToken cancellationToken)
    {
        // Query complexa com JOIN para trazer o produto e suas variantes de uma só vez
        const string sql = @"
            SELECT p.*, pv.*
            FROM product p
            LEFT JOIN product_variant pv ON p.product_id = pv.product_id
            WHERE p.slug = @Slug AND p.deleted_at IS NULL;
        ";

        var productDictionary = new Dictionary<Guid, Product>();

        await _uow.Connection.QueryAsync<ProductDataModel, ProductVariantDataModel, Product>(
            sql,
            (productData, variantData) =>
            {
                if (!productDictionary.TryGetValue(productData.product_id, out var productEntry))
                {
                    productEntry = Product.With(
                        productData.product_id, productData.name, productData.slug, productData.description,
                        productData.base_price, productData.stock_quantity, productData.is_active,
                        productData.category_id, productData.brand_id
                    );
                    productDictionary.Add(productEntry.Id, productEntry);
                }

                if (variantData is not null)
                {
                    var variant = ProductVariant.With(
                        variantData.product_variant_id, variantData.product_id, variantData.sku,
                        variantData.color_id, variantData.size_id, variantData.stock_quantity,
                        variantData.additional_price, variantData.is_active
                    );
                    productEntry.AddVariant(variant);
                }

                return productEntry;
            },
            new { Slug = slug },
            splitOn: "product_variant_id",
            transaction: _uow.HasActiveTransaction ? _uow.Transaction : null
        );

        return productDictionary.Values.FirstOrDefault();
        
    }

    public Task<PagedResult<Product>> SearchAsync(ProductSearchQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}