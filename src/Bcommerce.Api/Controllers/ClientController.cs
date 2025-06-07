using Bcomerce.Application.UseCases.Clients.Create;
using Bcommerce.Domain.Validations;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.Api.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientController : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(CreateClientOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(List<Error>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] CreateClientInput input, [FromServices] ICreateClientUseCase useCase)
    {
        var result = await useCase.Execute(input);
        return Ok();
    }
}