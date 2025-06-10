using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Common;
using Bcommerce.Domain.Categories.Repositories;
using Bcommerce.Domain.Validations;
using Bcommerce.Domain.Validations.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcomerce.Application.UseCases.Catalog.UpdateCategory
{
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
            var category = await _categoryRepository.Get(input.Id, CancellationToken.None);

            if (category is null)
            {
                notification.Append(new Error("Categoria não encontrada."));
                return Result<CategoryOutput, Notification>.Fail(notification);
            }

            category.Update(input.Name, input.Slug, input.Description, input.ParentCategoryId, input.IsActive, input.SortOrder);
            category.Validate(notification);

            if (notification.HasError())
            {
                return Result<CategoryOutput, Notification>.Fail(notification);
            }

            await _uow.Begin();
            await _categoryRepository.Update(category, CancellationToken.None);
            await _uow.Commit();

            return Result<CategoryOutput, Notification>.Ok(CategoryOutput.FromCategory(category));
        }
    }
}
