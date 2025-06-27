using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Products.GetProductById;

public interface IGetProductByIdUseCase
    : IUseCase<Guid, ProductOutput, Notification> // Input: Guid (Id do produto), Output: ProductOutput
{
}