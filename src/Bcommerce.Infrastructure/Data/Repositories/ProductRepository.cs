using Bcommerce.Domain.Catalog.Products;
using Bcommerce.Domain.Catalog.Products.Entities;
using Bcommerce.Domain.Catalog.Products.Repositories;
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
            
            return await QueryAndHydrateProduct(sql, new { Id = id });
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

            return await QueryAndHydrateProduct(sql, new { Slug = slug });
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
            
            // Inserir entidades filhas
            await InsertImages(aggregate.Images, cancellationToken);
            await InsertVariants(aggregate.Variants, cancellationToken);
        }
        
        public async Task Update(Product aggregate, CancellationToken cancellationToken)
        {
            // Lógica de Update é mais complexa:
            // 1. Atualiza a entidade principal 'products'.
            // 2. Deleta (ou soft-delete) imagens/variantes que não existem mais no agregado.
            // 3. Atualiza imagens/variantes existentes.
            // 4. Insere novas imagens/variantes.
            // Tudo isso deve ocorrer dentro da transação do UoW.
            // Por simplicidade, vamos focar no Get e Insert que são os mais didáticos.
            throw new NotImplementedException("Update de agregados complexos requer uma estratégia de sincronização.");
        }

        public Task Delete(Product aggregate, CancellationToken cancellationToken)
        {
            // Implementaria o soft-delete na tabela principal
            const string sql = "UPDATE products SET deleted_at = @Now WHERE product_id = @Id;";
            return _uow.Connection.ExecuteAsync(new CommandDefinition(sql, new { aggregate.Id, Now = DateTime.UtcNow }, _uow.Transaction, cancellationToken: cancellationToken));
        }

        // Método auxiliar que executa a query e hidrata o agregado
        private async Task<Product?> QueryAndHydrateProduct(string sql, object param)
        {
            var productDict = new Dictionary<Guid, Product>();

            await _uow.Connection.QueryAsync<ProductDataModel, ProductImageDataModel, ProductVariantDataModel, bool>(
                sql,
                (productData, imageData, variantData) =>
                {
                    if (!productDict.TryGetValue(productData.product_id, out var product))
                    {
                        product = HydrateProduct(productData);
                        productDict.Add(product.Id, product);
                    }

                    if (imageData != null && product.Images.All(i => i.Id != imageData.Id))
                    {
                        product.AddImage(HydrateImage(imageData));
                    }

                    if (variantData != null && product.Variants.All(v => v.Id != variantData.Id))
                    {
                        product.AddVariant(HydrateVariant(variantData));
                    }
                    
                    return true;
                },
                param,
                transaction: _uow.Transaction,
                splitOn: "Id,Id" // Dapper divide o resultado com base nos IDs das tabelas
            );
            return productDict.Values.FirstOrDefault();
        }

        // Métodos de hidratação (reconstroem as entidades do domínio a partir dos Data Models)
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
                new List<ProductImage>(), new List<ProductVariant>() // Inicia coleções vazias
            );
        }

        private static ProductImage HydrateImage(ProductImageDataModel model)
        {
            return ProductImage.With(model.Id, model.product_id, model.image_url, model.alt_text, model.is_cover, model.sort_order);
        }

        private static ProductVariant HydrateVariant(ProductVariantDataModel model)
        {
            return ProductVariant.With(model.Id, model.product_id, model.sku, model.color_id, model.size_id, model.stock_quantity, Money.Create(model.additional_price), model.is_active);
        }

        // Métodos auxiliares para inserir coleções
        private async Task InsertImages(IEnumerable<ProductImage> images, CancellationToken cancellationToken)
        {
            const string imageSql = @"
                INSERT INTO product_images (product_image_id, product_id, image_url, alt_text, is_cover, sort_order)
                VALUES (@Id, @ProductId, @ImageUrl, @AltText, @IsCover, @SortOrder);
            ";
            foreach (var image in images)
            {
                await _uow.Connection.ExecuteAsync(new CommandDefinition(imageSql, image, _uow.Transaction, cancellationToken: cancellationToken));
            }
        }
        
        private async Task InsertVariants(IEnumerable<ProductVariant> variants, CancellationToken cancellationToken)
        {
             const string variantSql = @"
                INSERT INTO product_variants (product_variant_id, product_id, sku, color_id, size_id, stock_quantity, additional_price, is_active)
                VALUES (@Id, @ProductId, @Sku, @ColorId, @SizeId, @StockQuantity, @AdditionalPriceAmount, @IsActive);
            ";
            foreach (var variant in variants)
            {
                await _uow.Connection.ExecuteAsync(new CommandDefinition(variantSql, new
                {
                    variant.Id, variant.ProductId, variant.Sku, variant.ColorId, variant.SizeId, variant.StockQuantity,
                    AdditionalPriceAmount = variant.AdditionalPrice.Amount,
                    variant.IsActive
                }, _uow.Transaction, cancellationToken: cancellationToken));
            }
        }

        // Demais métodos da interface (GetByBaseSkuAsync, SearchAsync) seguiriam a mesma lógica de query e hidratação.
        public Task<Product?> GetByBaseSkuAsync(string baseSku, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IEnumerable<Product>> SearchAsync(string searchTerm, int page, int pageSize, CancellationToken cancellationToken) => throw new NotImplementedException();
    }