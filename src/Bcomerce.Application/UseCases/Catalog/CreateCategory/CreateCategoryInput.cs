using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bcomerce.Application.UseCases.Catalog.CreateCategory;

public record CreateCategoryInput(
    string Name,
    string Slug,
    string? Description,
    Guid? ParentCategoryId
);


