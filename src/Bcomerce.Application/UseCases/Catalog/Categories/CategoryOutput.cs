using Bcommerce.Domain.Catalog.Categories;

namespace Bcomerce.Application.UseCases.Catalog.Categories;

public record CategoryOutput(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid? ParentCategoryId,
    bool IsActive,
    int SortOrder
) {
    public static CategoryOutput FromCategory(Category category)
    {
        return new CategoryOutput(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ParentCategoryId,
            category.IsActive,
            category.SortOrder
        );
    }
}