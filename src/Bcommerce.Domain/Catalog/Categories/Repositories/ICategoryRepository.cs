using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Catalog.Categories.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken);
}