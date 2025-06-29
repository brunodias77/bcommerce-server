using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;
using Bcomerce.Application.UseCases.Catalog.Products.ListPublicProducts;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Products.ListProducts;

public interface IListProductsUseCase : IUseCase<ListProductsInput, PagedListOutput<ProductOutput>, Notification> { }
