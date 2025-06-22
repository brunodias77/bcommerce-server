using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Catalog.Categories.Repositories;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Categories.ListCategories;

public class ListCategoriesUseCase : IListCategoriesUseCase
{
    private readonly ICategoryRepository _categoryRepository;

    public ListCategoriesUseCase(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<IEnumerable<CategoryOutput>, Notification>> Execute(ListCategoriesInput input)
    {
        // Em um cenário real, o repositório teria um método que aceita a paginação e busca.
        // Por simplicidade, vamos assumir que o método GetAllAsync já existe no repositório.
        var categories = await _categoryRepository.GetAllAsync(CancellationToken.None);

        var output = categories.Select(CategoryOutput.FromCategory);

        return Result<IEnumerable<CategoryOutput>, Notification>.Ok(output);
    }
}