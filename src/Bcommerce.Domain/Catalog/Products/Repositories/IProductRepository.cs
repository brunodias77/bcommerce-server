using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Catalog.Products.Repositories;

public interface IProductRepository : IRepository<Product>
{
    // Métodos específicos para produtos
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<Product?> GetByBaseSkuAsync(string baseSku, CancellationToken cancellationToken);
        
    // Para busca com Full-Text Search
    Task<IEnumerable<Product>> SearchAsync(string searchTerm, int page, int pageSize, CancellationToken cancellationToken);
    Task<IEnumerable<Product>> ListAsync(int page, int pageSize, string? searchTerm, string sortBy, string sortDirection, CancellationToken cancellationToken);

    
    
}
    