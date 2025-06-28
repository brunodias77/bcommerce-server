using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Products.GetPublicProduct;

public interface IGetPublicProductBySlugUseCase
    : IUseCase<string, PublicProductOutput, Notification> 
{
}
