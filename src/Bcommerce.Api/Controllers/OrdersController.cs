using Bcomerce.Application.UseCases.Marketing.Coupons.ApplyCoupon;
using Bcomerce.Application.UseCases.Sales.Orders;
using Bcomerce.Application.UseCases.Sales.Orders.CreateOrder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(OrderOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderInput input,
        [FromServices] ICreateOrderUseCase useCase)
    {
        var result = await useCase.Execute(input);

        if (result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status201Created, result.Value);
        }

        return BadRequest(new { errors = result.Error?.GetErrors() });
    }
    
    [HttpPost("{orderId:guid}/apply-coupon")]
    [ProducesResponseType(typeof(OrderOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyCoupon(
        [FromRoute] Guid orderId,
        [FromBody] ApplyCouponInput input, // Recebe apenas o { "couponCode": "CODE" }
        [FromServices] IApplyCouponUseCase useCase)
    {
        var fullInput = input with { OrderId = orderId };
        var result = await useCase.Execute(fullInput);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(new { errors = result.Error?.GetErrors() });
    }
}