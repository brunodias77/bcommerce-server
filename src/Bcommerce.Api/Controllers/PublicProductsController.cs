using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bcomerce.Application.UseCases.Catalog.Products.GetPublicProduct;
using Bcomerce.Application.UseCases.Catalog.Products.ListPublicProducts;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.Api.Controllers;

[ApiController]
[Route("api/products")]
public class PublicProductsController : ControllerBase
{
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(PublicProductOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductBySlug(
        [FromRoute] string slug,
        [FromServices] IGetPublicProductBySlugUseCase useCase)
    {
        var result = await useCase.Execute(slug);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NotFound(result.Error?.FirstError());
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedListOutput<PublicProductSummaryOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListProducts(
      [FromQuery] ListPublicProductsInput input,
      [FromServices] IListPublicProductsUseCase useCase)
    {
        var result = await useCase.Execute(input);
        return Ok(result.Value);
    }
}
