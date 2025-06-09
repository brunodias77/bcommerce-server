using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Common;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.CreateCategory;

public interface ICreateCategoryUseCase : IUseCase<CreateCategoryInput, CategoryOutput, Notification>
{

}