using Bcommerce.Domain.Abstractions;
using Bcommerce.Domain.Common;

namespace Bcommerce.Domain.Products.Repositories;

public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Busca um produto pelo seu slug, incluindo suas variantes e outras informações detalhadas.
    /// Usado na página de detalhes do produto.
    /// </summary>
    Task<Product?> GetBySlugWithDetailsAsync(string slug, CancellationToken cancellationToken);

    /// <summary>
    /// Realiza uma busca paginada e filtrada de produtos.
    /// Usado nas páginas de listagem de produtos, busca, etc.
    /// </summary>
    Task<PagedResult<Product>> SearchAsync(ProductSearchQuery query, CancellationToken cancellationToken);  
}