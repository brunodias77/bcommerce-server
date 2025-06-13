using Bcomerce.Application.UseCases.Catalog.GetProductDetails;
using Bcommerce.Domain.Validations;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(Bcomerce.Application.UseCases.Catalog.Common.ProductDetailsOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(List<Error>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(
        [FromRoute] string slug,
        [FromServices] IGetProductDetailsUseCase useCase)
    {
        var input = new GetProductDetailsInput(slug);
        var result = await useCase.Execute(input);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NotFound(result.Error?.GetErrors()); // Retorna 404 se o produto não for encontrado
    }
}