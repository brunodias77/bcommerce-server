using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Common;
using Bcommerce.Domain.Categories.Repositories;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.ListCategories
{
    public class ListCategoriesUseCase : IListCategoriesUseCase
    {
        private readonly ICategoryRepository _categoryRepository;
        public ListCategoriesUseCase(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Result<IEnumerable<CategoryOutput>, Notification>> Execute(object input)
        {
            var categories = await _categoryRepository.GetAllAsync(CancellationToken.None);
            var output = categories.Select(CategoryOutput.FromCategory);
            return Result<IEnumerable<CategoryOutput>, Notification>.Ok(output);
        }
    }
}
