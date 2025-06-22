using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Catalog.Categories.Repositories;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcomerce.Application.UseCases.Catalog.Categories.UpdateCategory;

public class UpdateCategoryUseCase : IUpdateCategoryUseCase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _uow;

    public UpdateCategoryUseCase(ICategoryRepository categoryRepository, IUnitOfWork uow)
    {
        _categoryRepository = categoryRepository;
        _uow = uow;
    }

    public async Task<Result<CategoryOutput, Notification>> Execute(UpdateCategoryInput input)
    {
        var notification = Notification.Create();
        var category = await _categoryRepository.Get(input.CategoryId, CancellationToken.None);

        if (category is null)
        {
            notification.Append(new Error("Categoria não encontrada."));
            return Result<CategoryOutput, Notification>.Fail(notification);
        }
        
        // Atualiza a entidade de domínio com os novos dados
        category.Update(input.Name, input.Description, input.SortOrder, notification);

        if (notification.HasError())
        {
            return Result<CategoryOutput, Notification>.Fail(notification);
        }

        await _uow.Begin();
        try
        {
            await _categoryRepository.Update(category, CancellationToken.None);
            await _uow.Commit();
        }
        catch (Exception)
        {
            await _uow.Rollback();
            notification.Append(new Error("Ocorreu um erro no banco de dados ao atualizar a categoria."));
            return Result<CategoryOutput, Notification>.Fail(notification);
        }

        return Result<CategoryOutput, Notification>.Ok(CategoryOutput.FromCategory(category));
    }
}