using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Catalog.Products.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken);
}