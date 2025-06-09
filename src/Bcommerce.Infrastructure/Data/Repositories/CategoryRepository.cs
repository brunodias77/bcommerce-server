using Bcommerce.Domain.Categories;
using Bcommerce.Domain.Categories.Repositories;
using Bcommerce.Infrastructure.Data.Models;
using Dapper;

namespace Bcommerce.Infrastructure.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly IUnitOfWork _uow;

    public CategoryRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task Insert(Category aggregate, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO category (category_id, name, slug, description, parent_category_id, is_active, sort_order, created_at, updated_at)
            VALUES (@Id, @Name, @Slug, @Description, @ParentCategoryId, @IsActive, @SortOrder, @CreatedAt, @UpdatedAt);
        ";
        await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, aggregate, _uow.Transaction, cancellationToken: cancellationToken));

    }

    public async Task<Category> Get(Guid id, CancellationToken cancellationToken)
    {
        const string sql = "SELECT * FROM category WHERE category_id = @Id AND deleted_at IS NULL;";
        var model = await _uow.Connection.QuerySingleOrDefaultAsync<CategoryDataModel>(
            sql,
            new { Id = id },
            transaction: _uow.HasActiveTransaction ? _uow.Transaction : null
        );
        return model is null ? null : Hydrate(model);
    }

    public async Task Delete(Category aggregate, CancellationToken cancellationToken)
    {
        // Implementando como Soft Delete
        const string sql = "UPDATE category SET deleted_at = @Now WHERE category_id = @Id;";
        await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, new { aggregate.Id, Now = DateTime.UtcNow }, _uow.Transaction, cancellationToken: cancellationToken));

    }

    public async Task Update(Category aggregate, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE category SET
                name = @Name,
                slug = @Slug,
                description = @Description,
                parent_category_id = @ParentCategoryId,
                is_active = @IsActive,
                sort_order = @SortOrder,
                updated_at = @UpdatedAt
            WHERE category_id = @Id AND deleted_at IS NULL;
        ";
        await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, aggregate, _uow.Transaction, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = "SELECT * FROM category WHERE deleted_at IS NULL ORDER BY sort_order, name;";
        var models = await _uow.Connection.QueryAsync<CategoryDataModel>(sql,
            transaction: _uow.HasActiveTransaction ? _uow.Transaction : null
        );
        return models.Select(Hydrate);
    }

    private static Category Hydrate(CategoryDataModel model)
    {
        return Category.With(
            model.category_id,
            model.name,
            model.slug,
            model.description,
            model.parent_category_id,
            model.is_active,
            model.sort_order,
            model.created_at,
            model.updated_at
        );
    }
}