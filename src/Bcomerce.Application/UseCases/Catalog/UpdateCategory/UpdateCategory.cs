using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bcomerce.Application.UseCases.Catalog.UpdateCategory
{
    public record UpdateCategoryInput(
        Guid Id,
        string Name,
        string Slug,
        string? Description,
        Guid? ParentCategoryId,
        bool IsActive,
        int SortOrder
    );
}
