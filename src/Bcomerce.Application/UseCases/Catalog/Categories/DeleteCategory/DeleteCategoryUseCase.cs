using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Catalog.Categories.Repositories;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcomerce.Application.UseCases.Catalog.Categories.DeleteCategory;

public class DeleteCategoryUseCase : IDeleteCategoryUseCase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _uow;

    public DeleteCategoryUseCase(ICategoryRepository categoryRepository, IUnitOfWork uow)
    {
        _categoryRepository = categoryRepository;
        _uow = uow;
    }

    public async Task<Result<bool, Notification>> Execute(Guid categoryId)
    {
        var notification = Notification.Create();
        var category = await _categoryRepository.Get(categoryId, CancellationToken.None);

        if (category is null)
        {
            notification.Append(new Error("Categoria não encontrada."));
            return Result<bool, Notification>.Fail(notification);
        }

        // Aqui, a lógica de soft delete é chamada no repositório
        await _uow.Begin();
        try
        {
            await _categoryRepository.Delete(category, CancellationToken.None);
            await _uow.Commit();
        }
        catch (Exception)
        {
            await _uow.Rollback();
            notification.Append(new Error("Ocorreu um erro no banco de dados ao excluir a categoria."));
            return Result<bool, Notification>.Fail(notification);
        }

        return Result<bool, Notification>.Ok(true);
    }
}