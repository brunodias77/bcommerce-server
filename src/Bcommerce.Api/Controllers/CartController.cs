using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bcomerce.Application.UseCases.Sales.Carts.AddItemToCart;
using Bcomerce.Application.UseCases.Sales.Carts.GetCart;
using Bcomerce.Application.UseCases.Sales.Carts.RemoveCartItem;
using Bcomerce.Application.UseCases.Sales.Carts.UpdateCartItemQuantity;
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
    
    // NOVO ENDPOINT
    [HttpGet]
    [ProducesResponseType(typeof(CartOutput), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCart(
        [FromServices] IGetCartUseCase useCase)
    {
        var result = await useCase.Execute(null);
        // Este caso de uso sempre retorna sucesso, mesmo com carrinho vazio.
        return Ok(result.Value);
    }
    
    // NOVO ENDPOINT
    [HttpPut("items/{cartItemId:guid}")]
    [ProducesResponseType(typeof(CartOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateItemQuantity(
        [FromRoute] Guid cartItemId,
        [FromBody] UpdateCartItemQuantityInput input, // Recebe apenas a nova quantidade
        [FromServices] IUpdateCartItemQuantityUseCase useCase)
    {
        var fullInput = input with { CartItemId = cartItemId };
        var result = await useCase.Execute(fullInput);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(new { errors = result.Error?.GetErrors() });
    }
    
    // NOVO ENDPOINT
    [HttpDelete("items/{cartItemId:guid}")]
    [ProducesResponseType(typeof(CartOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveItemFromCart(
        [FromRoute] Guid cartItemId,
        [FromServices] IRemoveCartItemUseCase useCase)
    {
        var result = await useCase.Execute(cartItemId);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        
        return BadRequest(new { errors = result.Error?.GetErrors() });
    }
}