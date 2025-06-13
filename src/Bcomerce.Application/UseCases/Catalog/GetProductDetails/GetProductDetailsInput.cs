namespace Bcomerce.Application.UseCases.Catalog.GetProductDetails;
/// <summary>
/// Input para o caso de uso de busca de detalhes de um produto.
/// </summary>
/// <param name="Slug">O slug (URL amigável) do produto a ser buscado.</param>
public record GetProductDetailsInput(string Slug);