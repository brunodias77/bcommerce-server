using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Categories.ListCategories;

public interface IListCategoriesUseCase 
    : IUseCase<ListCategoriesInput, IEnumerable<CategoryOutput>, Notification> {}