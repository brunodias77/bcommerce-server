using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;

public interface ICreateProductUseCase 
    : IUseCase<CreateProductInput, ProductOutput, Notification>
{
}