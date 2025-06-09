using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bcomerce.Application.UseCases.Catalog.DeleteCategories
{
    interface IDeleteCategoryUseCase : IUseCase<Guid, bool, Notification>
    {
    }
}
