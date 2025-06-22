using Bcomerce.Application.UseCases.Catalog.Categories;
using Bcomerce.Application.UseCases.Catalog.Categories.CreateCategory;
using Bcomerce.Application.UseCases.Catalog.Categories.DeleteCategory;
using Bcomerce.Application.UseCases.Catalog.Categories.GetCategoryById;
using Bcomerce.Application.UseCases.Catalog.Categories.ListCategories;
using Bcomerce.Application.UseCases.Catalog.Categories.UpdateCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.Api.Controllers;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Admin")] // Protege todos os endpoints deste controller
public class CategoriesController : ControllerBase
{
    /// <summary>
    /// Cria uma nova categoria de produto.
    /// </summary>
    /// <param name="input">Dados para a nova categoria.</param>
    /// <param name="useCase">O caso de uso para criar a categoria.</param>
    /// <returns>Os dados da categoria recém-criada.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryInput input,
        [FromServices] ICreateCategoryUseCase useCase)
    {
        var result = await useCase.Execute(input);

        if (result.IsSuccess)
        {
            // Retorna 201 Created com a localização do novo recurso e os dados criados.
            return CreatedAtAction(nameof(GetCategoryById), new { categoryId = result.Value!.Id }, result.Value);
        }

        return BadRequest(new { errors = result.Error?.GetErrors() });
    }
    
    
    [HttpGet("{categoryId:guid}", Name = "GetCategoryById")]
    [ProducesResponseType(typeof(CategoryOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(
        [FromRoute] Guid categoryId,
        [FromServices] IGetCategoryByIdUseCase useCase)
    {
        var result = await useCase.Execute(categoryId);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error?.FirstError());
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListCategories(
        [FromQuery] ListCategoriesInput input,
        [FromServices] IListCategoriesUseCase useCase)
    {
        var result = await useCase.Execute(input);
        return Ok(result.Value);
    }
    
    [HttpPut("{categoryId:guid}")]
    [ProducesResponseType(typeof(CategoryOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(
        [FromRoute] Guid categoryId,
        [FromBody] UpdateCategoryInput payload,
        [FromServices] IUpdateCategoryUseCase useCase)
    {
        // Garante que o ID da rota seja o mesmo do payload para consistência
        if (categoryId != payload.CategoryId)
        {
            return BadRequest(new { errors = new[] { new { message = "ID da rota diverge do ID do payload." } } });
        }
        
        var result = await useCase.Execute(payload);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { errors = result.Error?.GetErrors() });
    }

    [HttpDelete("{categoryId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteCategory(
        [FromRoute] Guid categoryId,
        [FromServices] IDeleteCategoryUseCase useCase)
    {
        var result = await useCase.Execute(categoryId);
        return result.IsSuccess ? NoContent() : BadRequest(new { errors = result.Error?.GetErrors() });
    }
    
    // Endpoint de apoio para o CreatedAtAction. Será implementado a seguir.
    // Por enquanto, apenas um placeholder para evitar erros de compilação.
    [HttpGet("{categoryId:guid}", Name = "GetCategoryById")]
    [ApiExplorerSettings(IgnoreApi = true)] // Oculta do Swagger por enquanto
    public IActionResult GetCategoryById(Guid categoryId)
    {
        // A implementação completa virá com o GetCategoryByIdUseCase
        return Ok(new { Id = categoryId, Message = "Endpoint a ser implementado." });
    }
}