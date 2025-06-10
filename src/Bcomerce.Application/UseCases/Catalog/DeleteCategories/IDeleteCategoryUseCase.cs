using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Validations.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.DeleteCategories
{
    public interface IDeleteCategoryUseCase : IUseCase<Guid, bool, Notification>
    {
    }
}
