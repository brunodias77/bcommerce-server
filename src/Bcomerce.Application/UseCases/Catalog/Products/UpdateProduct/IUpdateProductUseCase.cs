using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Products.UpdateProduct;

public interface IUpdateProductUseCase
    : IUseCase<UpdateProductInput, ProductOutput, Notification>
{
}