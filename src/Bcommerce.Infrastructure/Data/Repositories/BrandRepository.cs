
using Bcommerce.Domain.Catalog.Brands;
using Bcommerce.Domain.Catalog.Brands.Repositories;
using Bcommerce.Infrastructure.Data.Models;
using Dapper;

namespace Bcommerce.Infrastructure.Data.Repositories
{
     public class BrandRepository : IBrandRepository
    {
        private readonly IUnitOfWork _uow;

        public BrandRepository(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task Insert(Brand aggregate, CancellationToken cancellationToken)
        {
            const string sql = @"
            INSERT INTO brands (brand_id, name, slug, description, logo_url, is_active, created_at, updated_at, version)
            VALUES (@Id, @Name, @Slug, @Description, @LogoUrl, @IsActive, @CreatedAt, @UpdatedAt, 1);
        ";
            await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, aggregate, _uow.Transaction, cancellationToken: cancellationToken));
        }

        public async Task<Brand?> Get(Guid id, CancellationToken cancellationToken)
        {
            const string sql = "SELECT * FROM brands WHERE brand_id = @Id AND deleted_at IS NULL;";
            var model = await _uow.Connection.QuerySingleOrDefaultAsync<BrandDataModel>(
                sql,
                new { Id = id },
                transaction: _uow.HasActiveTransaction ? _uow.Transaction : null
            );
            return model is null ? null : Hydrate(model);
        }

        public async Task Update(Brand aggregate, CancellationToken cancellationToken)
        {
            const string sql = @"
            UPDATE brands SET
                name = @Name,
                slug = @Slug,
                description = @Description,
                logo_url = @LogoUrl,
                is_active = @IsActive,
                updated_at = @UpdatedAt,
                version = version + 1
            WHERE brand_id = @Id AND deleted_at IS NULL;
        ";
            await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, aggregate, _uow.Transaction, cancellationToken: cancellationToken));
        }

        public async Task Delete(Brand aggregate, CancellationToken cancellationToken)
        {
            const string sql = "UPDATE brands SET deleted_at = @Now WHERE brand_id = @Id;";
            await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, new { aggregate.Id, Now = DateTime.UtcNow }, _uow.Transaction, cancellationToken: cancellationToken));
        }

        // MÉTODO ADICIONADO
        public async Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            const string sql = "SELECT * FROM brands WHERE slug = @Slug AND deleted_at IS NULL;";
            var model = await _uow.Connection.QuerySingleOrDefaultAsync<BrandDataModel>(
                sql,
                new { Slug = slug },
                transaction: _uow.HasActiveTransaction ? _uow.Transaction : null
            );
            return model is null ? null : Hydrate(model);
        }

        // MÉTODO ADICIONADO
        public async Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken)
        {
            const string sql = "SELECT 1 FROM brands WHERE name = @Name AND deleted_at IS NULL;";
            var result = await _uow.Connection.ExecuteScalarAsync<int?>(
                sql,
                new { Name = name },
                transaction: _uow.HasActiveTransaction ? _uow.Transaction : null
            );
            return result.HasValue;
        }

        private static Brand Hydrate(BrandDataModel model)
        {
            return Brand.With(
                model.brand_id,
                model.name,
                model.slug,
                model.description,
                model.logo_url,
                model.is_active,
                model.created_at,
                model.updated_at
            );
        }
    }
}
