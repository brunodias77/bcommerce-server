using Bcommerce.Domain.Abstractions;

namespace Bcommerce.Domain.Categories.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    // Podemos adicionar métodos específicos para categorias no futuro, se necessário.
    // Ex: Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    // Ex: Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken);
    Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken);

}