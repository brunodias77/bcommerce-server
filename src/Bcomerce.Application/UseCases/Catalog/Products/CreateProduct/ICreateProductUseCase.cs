using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;

public interface ICreateProductUseCase 
    : IUseCase<CreateProductInput, CreateProductOutput, Notification>
{
}