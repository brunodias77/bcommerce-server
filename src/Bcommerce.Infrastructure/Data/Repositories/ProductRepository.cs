using System.Text;
using Bcommerce.Domain.Catalog.Products;
using Bcommerce.Domain.Catalog.Products.Entities;
using Bcommerce.Domain.Catalog.Products.Repositories;
using Bcommerce.Domain.Catalog.Products.ValueObjects;
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
    
    // ... todos os outros métodos do repositório permanecem os mesmos ...
    
    public async Task<Product?> Get(Guid id, CancellationToken cancellationToken)
    {
        const string sql = @"
                SELECT 
                    p.*,
                    pi.product_image_id as Id, pi.*,
                    pv.product_variant_id as Id, pv.*
                FROM products p
                LEFT JOIN product_images pi ON p.product_id = pi.product_id AND pi.deleted_at IS NULL
                LEFT JOIN product_variants pv ON p.product_id = pv.product_id AND pv.deleted_at IS NULL
                WHERE p.product_id = @Id AND p.deleted_at IS NULL;
            ";

        var product = await QueryAndHydrateProducts(sql, new { Id = id });
        return product.FirstOrDefault();
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        const string sql = @"
                SELECT 
                    p.*,
                    pi.product_image_id as Id, pi.*,
                    pv.product_variant_id as Id, pv.*
                FROM products p
                LEFT JOIN product_images pi ON p.product_id = pi.product_id AND pi.deleted_at IS NULL
                LEFT JOIN product_variants pv ON p.product_id = pv.product_id AND pv.deleted_at IS NULL
                WHERE p.slug = @Slug AND p.deleted_at IS NULL;
            ";

        var product = await QueryAndHydrateProducts(sql, new { Slug = slug });
        return product.FirstOrDefault();
    }

    public async Task Insert(Product aggregate, CancellationToken cancellationToken)
    {
        const string productSql = @"
                INSERT INTO products (product_id, base_sku, name, slug, description, category_id, brand_id, base_price, sale_price, sale_price_start_date, sale_price_end_date, stock_quantity, is_active, weight_kg, height_cm, width_cm, depth_cm, created_at, updated_at, version)
                VALUES (@Id, @BaseSku, @Name, @Slug, @Description, @CategoryId, @BrandId, @BasePriceAmount, @SalePriceAmount, @SalePriceStartDate, @SalePriceEndDate, @StockQuantity, @IsActive, @Weight, @Height, @Width, @Depth, @CreatedAt, @UpdatedAt, 1);
            ";

        await _uow.Connection.ExecuteAsync(new CommandDefinition(productSql, new
        {
            aggregate.Id,
            aggregate.BaseSku,
            aggregate.Name,
            aggregate.Slug,
            aggregate.Description,
            aggregate.CategoryId,
            aggregate.BrandId,
            BasePriceAmount = aggregate.BasePrice.Amount,
            SalePriceAmount = aggregate.SalePrice?.Amount,
            aggregate.SalePriceStartDate,
            aggregate.SalePriceEndDate,
            aggregate.StockQuantity,
            aggregate.IsActive,
            Weight = aggregate.Dimensions.WeightKg,
            Height = aggregate.Dimensions.HeightCm,
            Width = aggregate.Dimensions.WidthCm,
            Depth = aggregate.Dimensions.DepthCm,
            aggregate.CreatedAt,
            aggregate.UpdatedAt
        }, _uow.Transaction, cancellationToken: cancellationToken));

        await InsertImages(aggregate.Images, cancellationToken);
        await InsertVariants(aggregate.Variants, cancellationToken);
    }

    public async Task Update(Product aggregate, CancellationToken cancellationToken)
    {
        // Por enquanto, vamos atualizar apenas os dados da tabela principal 'products'.
        // A atualização de imagens e variantes seria feita em UseCases específicos.
        const string productSql = @"
                UPDATE products SET
                    name = @Name,
                    slug = @Slug,
                    description = @Description,
                    base_price = @BasePriceAmount,
                    stock_quantity = @StockQuantity,
                    is_active = @IsActive,
                    category_id = @CategoryId,
                    brand_id = @BrandId,
                    weight_kg = @Weight,
                    height_cm = @Height,
                    width_cm = @Width,
                    depth_cm = @Depth,
                    updated_at = @UpdatedAt,
                    version = version + 1
                WHERE product_id = @Id;
            ";

        await _uow.Connection.ExecuteAsync(new CommandDefinition(productSql, new
        {
            aggregate.Id,
            aggregate.Name,
            aggregate.Slug,
            aggregate.Description,
            BasePriceAmount = aggregate.BasePrice.Amount,
            aggregate.StockQuantity,
            aggregate.IsActive,
            aggregate.CategoryId,
            aggregate.BrandId,
            Weight = aggregate.Dimensions.WeightKg,
            Height = aggregate.Dimensions.HeightCm,
            Width = aggregate.Dimensions.WidthCm,
            Depth = aggregate.Dimensions.DepthCm,
            aggregate.UpdatedAt
        }, _uow.Transaction, cancellationToken: cancellationToken));

        // A lógica de sincronizar coleções (imagens, variantes) seria mais complexa
        // e viria aqui se necessário.
    }

    public Task Delete(Product aggregate, CancellationToken cancellationToken)
    {
        const string sql = "UPDATE products SET deleted_at = @Now WHERE product_id = @Id;";
        return _uow.Connection.ExecuteAsync(new CommandDefinition(sql, new { aggregate.Id, Now = DateTime.UtcNow }, _uow.Transaction, cancellationToken: cancellationToken));
    }

    public async Task<Product?> GetByBaseSkuAsync(string baseSku, CancellationToken cancellationToken)
    {
        const string sql = @"
                SELECT 
                    p.*,
                    pi.product_image_id as Id, pi.*,
                    pv.product_variant_id as Id, pv.*
                FROM products p
                LEFT JOIN product_images pi ON p.product_id = pi.product_id AND pi.deleted_at IS NULL
                LEFT JOIN product_variants pv ON p.product_id = pv.product_id AND pv.deleted_at IS NULL
                WHERE p.base_sku = @BaseSku AND p.deleted_at IS NULL;
            ";
        var product = await QueryAndHydrateProducts(sql, new { BaseSku = baseSku });
        return product.FirstOrDefault();
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = string.Join(" & ", searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        const string sql = @"
                SELECT 
                    p.*,
                    pi.product_image_id as Id, pi.*,
                    pv.product_variant_id as Id, pv.*
                FROM products p
                LEFT JOIN product_images pi ON p.product_id = pi.product_id AND pi.deleted_at IS NULL
                LEFT JOIN product_variants pv ON p.product_id = pv.product_id AND pv.deleted_at IS NULL
                WHERE p.deleted_at IS NULL
                  AND p.search_vector @@ to_tsquery('portuguese', @Query)
                ORDER BY ts_rank(p.search_vector, to_tsquery('portuguese', @Query)) DESC
                LIMIT @PageSize OFFSET @Offset;
            ";

        return await QueryAndHydrateProducts(sql, new
        {
            Query = query,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        });
    }

    public async Task<IEnumerable<Product>> ListAsync(int page, int pageSize, string? searchTerm, Guid? categoryId, Guid? brandId, string sortBy, string sortDirection, CancellationToken cancellationToken)
    {
        // O código original já ignorava o SqlBuilder e usava esta query manual.
        // A refatoração aqui é simplesmente remover o código morto do SqlBuilder.
        var validSortColumns = new Dictionary<string, string> { ["name"] = "p.name", ["price"] = "p.base_price" };
        var orderBy = validSortColumns.ContainsKey(sortBy.ToLower()) ? validSortColumns[sortBy.ToLower()] : "p.name";
        var direction = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

        // NOTA: A query original para hidratação completa foi mantida, conforme a lógica do código anterior.
        var fullQuery = @"
        SELECT p.*, pi.product_image_id as Id, pi.*, pv.product_variant_id as Id, pv.*
        FROM products p
        LEFT JOIN product_images pi ON p.product_id = pi.product_id AND pi.deleted_at IS NULL AND pi.is_cover = TRUE
        LEFT JOIN product_variants pv ON p.product_id = pv.product_id AND pv.deleted_at IS NULL
        WHERE p.deleted_at IS NULL AND p.is_active = TRUE" +
            (categoryId.HasValue ? " AND p.category_id = @CategoryId" : "") +
            (brandId.HasValue ? " AND p.brand_id = @BrandId" : "") +
            (!string.IsNullOrWhiteSpace(searchTerm) ? " AND (p.name ILIKE @SearchTerm OR p.base_sku ILIKE @SearchTerm)" : "") +
            $" ORDER BY {orderBy} {direction} LIMIT @PageSize OFFSET @Offset";
            
        var parameters = new 
        { 
            SearchTerm = $"%{searchTerm}%", 
            CategoryId = categoryId, 
            BrandId = brandId, 
            PageSize = pageSize, 
            Offset = (page - 1) * pageSize 
        };

        return await QueryAndHydrateProducts(fullQuery, parameters);
    }

    public async Task<int> CountAsync(string? searchTerm, Guid? categoryId, Guid? brandId, CancellationToken cancellationToken)
    {
        // Refatorado para usar StringBuilder e DynamicParameters, removendo o SqlBuilder.
        var sql = new StringBuilder("SELECT COUNT(p.product_id) FROM products p WHERE p.deleted_at IS NULL AND p.is_active = TRUE");
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            sql.Append(" AND p.name ILIKE @SearchTerm");
            parameters.Add("SearchTerm", $"%{searchTerm}%");
        }
        if (categoryId.HasValue)
        {
            sql.Append(" AND p.category_id = @CategoryId");
            parameters.Add("CategoryId", categoryId.Value);
        }
        if (brandId.HasValue)
        {
            sql.Append(" AND p.brand_id = @BrandId");
            parameters.Add("BrandId", brandId.Value);
        }

        return await _uow.Connection.ExecuteScalarAsync<int>(sql.ToString(), parameters);
    }
    
    // HIDRATAÇÃO MULTI-QUERY (N+1 Otimizado)
    private async Task<IEnumerable<Product>> QueryAndHydrateProducts(string sql, object param)
    {
        var productDataModels = (await _uow.Connection.QueryAsync<ProductDataModel>(sql, param, _uow.Transaction)).ToList();
        if (!productDataModels.Any()) return Enumerable.Empty<Product>();

        var productIds = productDataModels.Select(p => p.product_id).ToList();

        const string imagesSql = "SELECT * FROM product_images WHERE product_id = ANY(@ProductIds) AND deleted_at IS NULL ORDER BY sort_order;";
        var imagesData = (await _uow.Connection.QueryAsync<ProductImageDataModel>(imagesSql, new { ProductIds = productIds }, _uow.Transaction))
            .ToLookup(i => i.product_id);

        const string variantsSql = "SELECT * FROM product_variants WHERE product_id = ANY(@ProductIds) AND deleted_at IS NULL;";
        var variantsData = (await _uow.Connection.QueryAsync<ProductVariantDataModel>(variantsSql, new { ProductIds = productIds }, _uow.Transaction))
            .ToLookup(v => v.product_id);

        return productDataModels.Select(productData =>
        {
            var images = imagesData[productData.product_id].Select(HydrateImage).ToList();
            var variants = variantsData[productData.product_id].Select(HydrateVariant).ToList();
            return HydrateProduct(productData, images, variants);
        });
    }

    private static Product HydrateProduct(ProductDataModel model, List<ProductImage> images, List<ProductVariant> variants)
    {
        return Product.With(
            model.product_id, model.base_sku, model.name, model.slug, model.description,
            Money.Create(model.base_price),
            model.sale_price.HasValue ? Money.Create(model.sale_price.Value) : null,
            model.sale_price_start_date, model.sale_price_end_date,
            model.stock_quantity, model.is_active,
            Dimensions.Create(model.weight_kg, model.height_cm, model.width_cm, model.depth_cm),
            model.category_id, model.brand_id,
            model.created_at, model.updated_at,
            images, variants
        );
    }

    private static ProductImage HydrateImage(ProductImageDataModel model)
    {
        // CORREÇÃO: A chamada agora corresponde à assinatura do método 'With' na entidade ProductImage
        return ProductImage.With(
            model.product_image_id, 
            model.product_id, 
            model.image_url, 
            model.alt_text, 
            model.is_cover, 
            model.sort_order
        );
    }

    private static ProductVariant HydrateVariant(ProductVariantDataModel model)
    {
        return ProductVariant.With(
            model.product_variant_id, model.product_id, model.sku, model.color_id, model.size_id,
            model.stock_quantity, Money.Create(model.additional_price),
            model.image_url, model.is_active
        );
    }

    private async Task InsertImages(IEnumerable<ProductImage> images, CancellationToken cancellationToken)
    {
        const string imageSql = @"
                INSERT INTO product_images (product_image_id, product_id, image_url, alt_text, is_cover, sort_order, version)
                VALUES (@Id, @ProductId, @ImageUrl, @AltText, @IsCover, @SortOrder, 1);
            ";
        foreach (var image in images)
        {
            await _uow.Connection.ExecuteAsync(new CommandDefinition(imageSql, image, _uow.Transaction, cancellationToken: cancellationToken));
        }
    }

    private async Task InsertVariants(IEnumerable<ProductVariant> variants, CancellationToken cancellationToken)
    {
        const string variantSql = @"
                INSERT INTO product_variants (product_variant_id, product_id, sku, color_id, size_id, stock_quantity, additional_price, image_url, is_active, version)
                VALUES (@Id, @ProductId, @Sku, @ColorId, @SizeId, @StockQuantity, @AdditionalPriceAmount, @ImageUrl, @IsActive, 1);
            ";
        foreach (var variant in variants)
        {
            await _uow.Connection.ExecuteAsync(new CommandDefinition(variantSql, new
            {
                variant.Id,
                variant.ProductId,
                variant.Sku,
                variant.ColorId,
                variant.SizeId,
                variant.StockQuantity,
                AdditionalPriceAmount = variant.AdditionalPrice.Amount,
                variant.ImageUrl,
                variant.IsActive
            }, _uow.Transaction, cancellationToken: cancellationToken));
        }
    }
}