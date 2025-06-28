using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bcomerce.Application.UseCases.Sales.Carts.AddItemToCart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize] // Carrinho é sempre para usuários logados
public class CartController : ControllerBase
{
    [HttpPost("items")]
    [ProducesResponseType(typeof(CartOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItemToCart(
        [FromBody] AddItemToCartInput input,
        [FromServices] IAddItemToCartUseCase useCase)
    {
        var result = await useCase.Execute(input);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(new { errors = result.Error?.GetErrors() });
    }
}