using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Catalog.Categories;
using Bcommerce.Domain.Catalog.Categories.Repositories;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcomerce.Application.UseCases.Catalog.Categories.CreateCategory;

public class CreateCategoryUseCase : ICreateCategoryUseCase
{
    public CreateCategoryUseCase(ICategoryRepository categoryRepository, IUnitOfWork uow)
    {
        _categoryRepository = categoryRepository;
        _uow = uow;
    }

    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _uow;
    public async Task<Result<CategoryOutput, Notification>> Execute(CreateCategoryInput input)
    {
        var notification = Notification.Create();
        
        // Regra de negócio: não permitir categorias com o mesmo nome.
        if (await _categoryRepository.ExistsWithNameAsync(input.Name, CancellationToken.None))
        {
            notification.Append(new Error($"Uma categoria com o nome '{input.Name}' já existe."));
            return Result<CategoryOutput, Notification>.Fail(notification);
        }
        
        var category = Category.NewCategory(
            input.Name,
            input.Description,
            input.ParentCategoryId,
            input.SortOrder,
            notification
        );
        
        if (notification.HasError())
        {
            return Result<CategoryOutput, Notification>.Fail(notification);
        }
        
        await _uow.Begin();
        try
        {
            await _categoryRepository.Insert(category, CancellationToken.None);
            await _uow.Commit();
        }
        catch (Exception)
        {
            await _uow.Rollback();
            notification.Append(new Error("Ocorreu um erro no banco de dados ao salvar a categoria."));
            return Result<CategoryOutput, Notification>.Fail(notification);
        }

        return Result<CategoryOutput, Notification>.Ok(CategoryOutput.FromCategory(category));
    }
}