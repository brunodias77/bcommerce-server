using Bcommerce.Domain.Catalog.Products;
using Bcommerce.Domain.Catalog.Products.Entities;
using Bcommerce.Domain.Catalog.Products.Repositories;
using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Common;
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
            
            var product = await QueryAndHydrateProduct(sql, new { Id = id });
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

            var product = await QueryAndHydrateProduct(sql, new { Slug = slug });
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
                aggregate.Id, aggregate.BaseSku, aggregate.Name, aggregate.Slug, aggregate.Description, aggregate.CategoryId, aggregate.BrandId,
                BasePriceAmount = aggregate.BasePrice.Amount,
                SalePriceAmount = aggregate.SalePrice?.Amount,
                aggregate.SalePriceStartDate, aggregate.SalePriceEndDate, aggregate.StockQuantity, aggregate.IsActive,
                Weight = aggregate.Dimensions.WeightKg,
                Height = aggregate.Dimensions.HeightCm,
                Width = aggregate.Dimensions.WidthCm,
                Depth = aggregate.Dimensions.DepthCm,
                aggregate.CreatedAt, aggregate.UpdatedAt
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
            var product = await QueryAndHydrateProduct(sql, new { BaseSku = baseSku });
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
            
            return await QueryAndHydrateProduct(sql, new
            {
                Query = query,
                PageSize = pageSize,
                Offset = (page - 1) * pageSize
            });
        }
        
        public async Task<IEnumerable<Product>> ListAsync(int page, int pageSize, string? searchTerm, string sortBy, string sortDirection, CancellationToken cancellationToken)
        {
            var baseSql = @"
                SELECT p.*,
                       pi.product_image_id as Id, pi.*,
                       pv.product_variant_id as Id, pv.*
                FROM products p
                LEFT JOIN product_images pi ON p.product_id = pi.product_id AND pi.is_cover = TRUE AND pi.deleted_at IS NULL
                LEFT JOIN product_variants pv ON p.product_id = pv.product_id AND pv.deleted_at IS NULL
                WHERE p.deleted_at IS NULL
            ";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                baseSql += " AND p.name ILIKE @SearchTerm ";
            }

            // Validação para evitar SQL Injection na ordenação
            var validSortColumns = new Dictionary<string, string>
            {
                ["name"] = "p.name",
                ["price"] = "p.base_price",
                ["sku"] = "p.base_sku"
            };
            var orderBy = validSortColumns.ContainsKey(sortBy.ToLower()) ? validSortColumns[sortBy.ToLower()] : "p.name";
            var direction = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";
    
            baseSql += $" ORDER BY {orderBy} {direction} LIMIT @PageSize OFFSET @Offset";
    
            var parameters = new
            {
                SearchTerm = $"%{searchTerm}%",
                PageSize = pageSize,
                Offset = (page - 1) * pageSize
            };

            return await QueryAndHydrateProduct(baseSql, parameters);
        }

        private async Task<IEnumerable<Product>> QueryAndHydrateProduct(string sql, object param)
        {
            var productDict = new Dictionary<Guid, Product>();

            await _uow.Connection.QueryAsync<ProductDataModel, ProductImageDataModel, ProductVariantDataModel, Product>(
                sql,
                (productData, imageData, variantData) =>
                {
                    if (!productDict.TryGetValue(productData.product_id, out var product))
                    {
                        product = HydrateProduct(productData);
                        productDict.Add(product.Id, product);
                    }

                    if (imageData != null && product.Images.All(i => i.Id != imageData.product_image_id))
                    {
                         var image = ProductImage.NewImage(imageData.product_id, imageData.image_url, imageData.alt_text, imageData.is_cover, imageData.sort_order);
                         // Adicionando a imagem diretamente à coleção interna (uma alternativa à exposição de um método Add)
                         (product.Images as List<ProductImage>)?.Add(image);
                    }

                    if (variantData != null && product.Variants.All(v => v.Id != variantData.product_variant_id))
                    {
                        // Adicionando a variante diretamente à coleção interna
                        (product.Variants as List<ProductVariant>)?.Add(HydrateVariant(variantData));
                    }
                    
                    return product;
                },
                param,
                transaction: _uow.HasActiveTransaction ? _uow.Transaction : null,
                splitOn: "Id,Id"
            );
            return productDict.Values;
        }

        private static Product HydrateProduct(ProductDataModel model)
        {
            return Product.With(
                model.product_id, model.base_sku, model.name, model.slug, model.description,
                Money.Create(model.base_price),
                model.sale_price.HasValue ? Money.Create(model.sale_price.Value) : null,
                model.sale_price_start_date, model.sale_price_end_date,
                model.stock_quantity, model.is_active,
                Dimensions.Create(model.weight_kg, model.height_cm, model.width_cm, model.depth_cm),
                model.category_id, model.brand_id,
                model.created_at, model.updated_at, // Parâmetros adicionados
                new List<ProductImage>(), new List<ProductVariant>()
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
                    variant.Id, variant.ProductId, variant.Sku, variant.ColorId, variant.SizeId, 
                    variant.StockQuantity,
                    AdditionalPriceAmount = variant.AdditionalPrice.Amount,
                    variant.ImageUrl,
                    variant.IsActive
                }, _uow.Transaction, cancellationToken: cancellationToken));
            }
        }
    }