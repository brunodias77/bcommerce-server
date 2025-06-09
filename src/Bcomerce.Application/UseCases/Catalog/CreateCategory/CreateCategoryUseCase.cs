using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Common;
using Bcommerce.Domain.Categories;
using Bcommerce.Domain.Categories.Repositories;
using Bcommerce.Domain.Validations;
using Bcommerce.Domain.Validations.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcomerce.Application.UseCases.Catalog.CreateCategory
{
    class CreateCategoryUseCase : ICreateCategoryUseCase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _uow;

        public CreateCategoryUseCase(ICategoryRepository categoryRepository, IUnitOfWork uow)
        {
            _categoryRepository = categoryRepository;
            _uow = uow;
        }

        public async Task<Result<CategoryOutput, Notification>> Execute(CreateCategoryInput input)
        {
            var notification = Notification.Create();

            // Adicionar verificação de slug duplicado (requer novo método no repositório)
            // var existingCategory = await _categoryRepository.GetBySlugAsync(input.Slug);
            // if (existingCategory is not null) { ... retornar erro ... }

            var category = Category.NewCategory(
                input.Name,
                input.Slug,
                input.Description,
                input.ParentCategoryId
            );

            category.Validate(notification);

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
            catch (Exception) // Idealmente, logar a exceção
            {
                await _uow.Rollback();
                notification.Append(new Error("Erro ao salvar a categoria."));
                return Result<CategoryOutput, Notification>.Fail(notification);
            }

            return Result<CategoryOutput, Notification>.Ok(CategoryOutput.FromCategory(category));
        }
    }
}
