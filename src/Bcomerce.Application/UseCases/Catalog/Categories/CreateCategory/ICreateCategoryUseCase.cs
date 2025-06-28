using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Categories.CreateCategory;

public interface ICreateCategoryUseCase 
    : IUseCase<CreateCategoryInput, CategoryOutput, Notification> {}