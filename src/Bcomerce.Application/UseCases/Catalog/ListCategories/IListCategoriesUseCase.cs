using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bcomerce.Application.Abstractions;
using Bcomerce.Application.UseCases.Catalog.Common;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.ListCategories
{
    public interface IListCategoriesUseCase : IUseCase<object, IEnumerable<CategoryOutput>, Notification>
    {
    }
}
