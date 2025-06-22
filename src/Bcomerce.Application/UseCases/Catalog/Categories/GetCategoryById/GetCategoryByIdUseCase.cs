using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Catalog.Categories.Repositories;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Categories.GetCategoryById;

public class GetCategoryByIdUseCase : IGetCategoryByIdUseCase
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdUseCase(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryOutput, Notification>> Execute(Guid categoryId)
    {
        var category = await _categoryRepository.Get(categoryId, CancellationToken.None);

        if (category is null)
        {
            var notification = Notification.Create().Append(new Error("Categoria não encontrada."));
            return Result<CategoryOutput, Notification>.Fail(notification);
        }

        return Result<CategoryOutput, Notification>.Ok(CategoryOutput.FromCategory(category));
    }
}