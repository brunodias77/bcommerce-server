using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bcommerce.Domain.Brands;
using Bcommerce.Domain.Brands.Repositories;
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
            INSERT INTO brand (brand_id, name, slug, description, logo_url, is_active, created_at, updated_at)
            VALUES (@Id, @Name, @Slug, @Description, @LogoUrl, @IsActive, @CreatedAt, @UpdatedAt);
        ";
            await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, aggregate, _uow.Transaction, cancellationToken: cancellationToken));
        }

        public async Task<Brand> Get(Guid id, CancellationToken cancellationToken)
        {
            const string sql = "SELECT * FROM brand WHERE brand_id = @Id AND deleted_at IS NULL;";
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
            UPDATE brand SET
                name = @Name,
                slug = @Slug,
                description = @Description,
                logo_url = @LogoUrl,
                is_active = @IsActive,
                updated_at = @UpdatedAt
            WHERE brand_id = @Id AND deleted_at IS NULL;
        ";
            await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, aggregate, _uow.Transaction, cancellationToken: cancellationToken));
        }

        public async Task Delete(Brand aggregate, CancellationToken cancellationToken)
        {
            const string sql = "UPDATE brand SET deleted_at = @Now WHERE brand_id = @Id;";
            await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, new { aggregate.Id, Now = DateTime.UtcNow }, _uow.Transaction, cancellationToken: cancellationToken));
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
