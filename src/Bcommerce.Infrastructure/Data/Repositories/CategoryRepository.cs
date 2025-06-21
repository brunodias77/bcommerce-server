using Bcommerce.Domain.Catalog.Categories;
using Bcommerce.Domain.Catalog.Categories.Repositories;
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
            INSERT INTO categories (category_id, name, slug, description, parent_category_id, is_active, sort_order, created_at, updated_at, version)
            VALUES (@Id, @Name, @Slug, @Description, @ParentCategoryId, @IsActive, @SortOrder, @CreatedAt, @UpdatedAt, 1);
        ";
        await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, aggregate, _uow.Transaction, cancellationToken: cancellationToken));
    }

    public async Task<Category?> Get(Guid id, CancellationToken cancellationToken)
    {
        const string sql = "SELECT * FROM categories WHERE category_id = @Id AND deleted_at IS NULL;";
        var model = await _uow.Connection.QuerySingleOrDefaultAsync<CategoryDataModel>(
            sql,
            new { Id = id },
            transaction: _uow.HasActiveTransaction ? _uow.Transaction : null
        );
        return model is null ? null : Hydrate(model);
    }

    public async Task Delete(Category aggregate, CancellationToken cancellationToken)
    {
        const string sql = "UPDATE categories SET deleted_at = @Now WHERE category_id = @Id;";
        await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, new { aggregate.Id, Now = DateTime.UtcNow }, _uow.Transaction, cancellationToken: cancellationToken));
    }

    public async Task Update(Category aggregate, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE categories SET
                name = @Name,
                slug = @Slug,
                description = @Description,
                parent_category_id = @ParentCategoryId,
                is_active = @IsActive,
                sort_order = @SortOrder,
                updated_at = @UpdatedAt,
                version = version + 1
            WHERE category_id = @Id AND deleted_at IS NULL;
        ";
        await _uow.Connection.ExecuteAsync(new CommandDefinition(sql, aggregate, _uow.Transaction, cancellationToken: cancellationToken));
    }

    // MÉTODO ADICIONADO
    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        const string sql = "SELECT * FROM categories WHERE slug = @Slug AND deleted_at IS NULL;";
        var model = await _uow.Connection.QuerySingleOrDefaultAsync<CategoryDataModel>(
            sql,
            new { Slug = slug },
            transaction: _uow.HasActiveTransaction ? _uow.Transaction : null
        );
        return model is null ? null : Hydrate(model);
    }

    // MÉTODO ADICIONADO
    public async Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken)
    {
        const string sql = "SELECT 1 FROM categories WHERE name = @Name AND deleted_at IS NULL;";
        var result = await _uow.Connection.ExecuteScalarAsync<int?>(
            sql,
            new { Name = name },
            transaction: _uow.HasActiveTransaction ? _uow.Transaction : null
        );
        return result.HasValue;
    }
    
    // Este método não faz parte da interface IRepository, mas é útil para casos de uso de listagem.
    // Pode ser movido para a interface se for um requisito comum.
    public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = "SELECT * FROM categories WHERE deleted_at IS NULL ORDER BY sort_order, name;";
        var models = await _uow.Connection.QueryAsync<CategoryDataModel>(sql,
            transaction: _uow.HasActiveTransaction ? _uow.Transaction : null
        );
        return models.Select(Hydrate);
    }

    private static Category Hydrate(CategoryDataModel model)
    {
        // A hidratação está correta e já inclui o sort_order
        return Category.With(
            model.category_id,
            model.name,
            model.slug,
            model.description,
            model.is_active,
            model.parent_category_id,
            model.sort_order, // <- Correto
            model.created_at,
            model.updated_at
        );
    }
}