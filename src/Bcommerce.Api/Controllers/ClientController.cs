using Bcomerce.Application.UseCases.Clients.Create;
using Bcomerce.Application.UseCases.Clients.GetMyProfile;
using Bcomerce.Application.UseCases.Clients.Login;
using Bcomerce.Application.UseCases.Clients.VerifyEmail;
using Bcommerce.Domain.Validations;
using Microsoft.AspNetCore.Authorization;
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
        if (result.IsSuccess)
        {
            // Retorne 201 Created com o output e a localização do novo recurso (opcional, mas boa prática)
            // return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
            return StatusCode(StatusCodes.Status201Created, result.Value);
        }

        // Se falhou, retorne 400 Bad Request com a lista de erros
        return BadRequest(result.Error?.GetErrors());
        
    }
    
    [HttpGet("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token, [FromServices] IVerifyEmailUseCase useCase)
    {
        var result = await useCase.Execute(token);
        if (result.IsSuccess)
        {
            return Ok(new { message = "E-mail verificado com sucesso!" });
        }
        return BadRequest(result.Error?.GetErrors());
    }
    
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginClientOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(List<Error>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginClientInput input, [FromServices] ILoginClientUseCase clientUseCase)
    {
        var result = await clientUseCase.Execute(input);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(result.Error?.GetErrors());
    }
    
    [Authorize] // Este atributo protege o endpoint!
    [HttpGet("me")]
    [ProducesResponseType(typeof(CreateClientOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyProfile([FromServices] IGetMyProfileUseCase useCase)
    {
        var result = await useCase.Execute(null);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(result.Error?.GetErrors());
    }
}