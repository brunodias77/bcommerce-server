using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Categories.GetCategoryById;

public interface IGetCategoryByIdUseCase 
    : IUseCase<Guid, CategoryOutput, Notification> {}