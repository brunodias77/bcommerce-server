using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Categories.DeleteCategory;

public interface IDeleteCategoryUseCase 
    : IUseCase<Guid, bool, Notification> {}