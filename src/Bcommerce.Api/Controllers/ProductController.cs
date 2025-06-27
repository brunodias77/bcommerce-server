using Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;
using Bcomerce.Application.UseCases.Catalog.Products.GetProductById;
using Bcomerce.Application.UseCases.Catalog.Products.ListProducts;
using Bcomerce.Application.UseCases.Catalog.Products.UpdateProduct;
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
    
    [HttpGet("{productId:guid}", Name = "GetProductById")]
    [ProducesResponseType(typeof(ProductOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(
        [FromRoute] Guid productId,
        [FromServices] IGetProductByIdUseCase useCase)
    {
        var result = await useCase.Execute(productId);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NotFound(result.Error?.FirstError());
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListProducts(
        [FromQuery] ListProductsInput input,
        [FromServices] IListProductsUseCase useCase)
    {
        var result = await useCase.Execute(input);
        
        // Neste caso, mesmo uma lista vazia é um sucesso.
        return Ok(result.Value);
    }
    
    [HttpPut("{productId:guid}")]
    [ProducesResponseType(typeof(ProductOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(
        [FromRoute] Guid productId,
        [FromBody] UpdateProductInput input, // Recebe os dados do corpo
        [FromServices] IUpdateProductUseCase useCase)
    {
        // Garante que o ID da rota é o mesmo do payload
        if (productId != input.ProductId)
        {
            return BadRequest(new { errors = new[] { "ID da rota diverge do ID do payload." } });
        }
        
        var result = await useCase.Execute(input);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        
        // Verifica se o erro foi "não encontrado" para retornar o status code correto
        if (result.Error?.GetErrors().Any(e => e.Message == "Produto não encontrado.") ?? false)
        {
            return NotFound(result.Error);
        }
        
        return BadRequest(new { errors = result.Error?.GetErrors() });
    }
}