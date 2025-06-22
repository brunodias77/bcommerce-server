namespace Bcomerce.Application.UseCases.Catalog.Categories.CreateCategory;

public record CreateCategoryInput(
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    int SortOrder
);