namespace Bcomerce.Application.UseCases.Catalog.Categories.ListCategories;

public record ListCategoriesInput(int Page = 1, int PageSize = 20, string? SearchTerm = null);
