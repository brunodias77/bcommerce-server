namespace Bcomerce.Application.UseCases.Catalog.Categories.UpdateCategory;

public record UpdateCategoryInput(
    Guid CategoryId,
    string Name,
    string? Description,
    int SortOrder
);