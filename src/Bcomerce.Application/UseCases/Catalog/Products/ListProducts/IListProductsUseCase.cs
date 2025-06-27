using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Products.ListProducts;

public interface IListProductsUseCase
    : IUseCase<ListProductsInput, IEnumerable<ProductOutput>, Notification>
{
}