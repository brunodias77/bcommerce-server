using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Categories.UpdateCategory;

public interface IUpdateCategoryUseCase 
    : IUseCase<UpdateCategoryInput, CategoryOutput, Notification> {}