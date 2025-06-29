using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Catalog.Categories.Repositories;
using Bcommerce.Domain.Validation;
using Bcommerce.Domain.Validation.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

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

        // --- LÓGICA DE VALIDAÇÃO DE NOME DUPLICADO CORRIGIDA ---
        // Verifica se o nome foi alterado E se o novo nome já existe.
        if (category.Name.ToLower() != input.Name.ToLower() && 
            await _categoryRepository.ExistsWithNameAsync(input.Name, CancellationToken.None))
        {
            notification.Append(new Error($"Uma categoria com o nome '{input.Name}' já existe."));
            return Result<CategoryOutput, Notification>.Fail(notification);
        }
        // --- FIM DA CORREÇÃO ---

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