using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Catalog.Brands.Repositories;

public interface IBrandRepository  : IRepository<Brand>
{
    Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken);
}