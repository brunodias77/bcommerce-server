using Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.Api.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin")] // Garante que só administradores acessem
public class ProductController : ControllerBase
{
    // O endpoint para criar o produto
    [HttpPost]
    [ProducesResponseType(typeof(ProductOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductInput input,
        [FromServices] ICreateProductUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.Execute(input);

        if (result.IsSuccess)
        {
            // Retorna 201 Created com a localização do novo recurso e os dados criados.
            // Para o CreatedAtAction funcionar, precisaremos de um endpoint "GetProductById".
            // Por enquanto, podemos retornar Created e o objeto.
            return StatusCode(StatusCodes.Status201Created, result.Value);
        }

        return BadRequest(new { errors = result.Error?.GetErrors() });
    }
}