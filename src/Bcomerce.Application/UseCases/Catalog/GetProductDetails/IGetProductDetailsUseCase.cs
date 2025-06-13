using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Common;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.GetProductDetails;

public interface IGetProductDetailsUseCase : IUseCase<GetProductDetailsInput, ProductDetailsOutput, Notification>

{
    
}