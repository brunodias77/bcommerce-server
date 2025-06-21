using Bcomerce.Application.UseCases.Catalog.Clients.Create;
using Bcomerce.Application.UseCases.Catalog.Clients.GetMyProfile;
using Bcomerce.Application.UseCases.Catalog.Clients.Login;
using Bcomerce.Application.UseCases.Catalog.Clients.RefreshToken;
using Bcomerce.Application.UseCases.Catalog.Clients.VerifyEmail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.Api.Controllers;

/// <summary>
/// Endpoints para gerenciamento e autenticação de clientes.
/// </summary>
[ApiController]
[Route("api/clients")]
public class ClientController : ControllerBase
{
    /// <summary>
    /// Registra um novo cliente no sistema.
    /// </summary>
    /// <param name="input">Dados para a criação do novo cliente.</param>
    /// <param name="useCase">O caso de uso para criar o cliente.</param>
    /// <returns>Os dados do cliente recém-criado.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(CreateClientOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] CreateClientInput input, [FromServices] ICreateClientUseCase useCase)
    {
        var result = await useCase.Execute(input);
        if (result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status201Created, result.Value);
        }

        // CORREÇÃO: Encapsula a lista de erros em um objeto para resolver a ambiguidade
        // e padronizar a resposta de erro da API.
        return BadRequest(new { errors = result.Error?.GetErrors() });
    }
    
    /// <summary>
    /// Verifica o e-mail de um cliente a partir de um token.
    /// </summary>
    /// <param name="token">O token de verificação recebido por e-mail.</param>
    /// <param name="useCase">O caso de uso para verificar o e-mail.</param>
    /// <returns>Uma mensagem de sucesso.</returns>
    [HttpGet("verify-email")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token, [FromServices] IVerifyEmailUseCase useCase)
    {
        var result = await useCase.Execute(token);
        if (result.IsSuccess)
        {
            return Ok(new { message = "E-mail verificado com sucesso!" });
        }
        
        // CORREÇÃO: Padroniza a resposta de erro.
        return BadRequest(new { errors = result.Error?.GetErrors() });
    }
    
    /// <summary>
    /// Autentica um cliente e retorna um token de acesso.
    /// </summary>
    /// <param name="input">As credenciais (e-mail e senha) do cliente.</param>
    /// <param name="useCase">O caso de uso para realizar o login.</param>
    /// <returns>O token de acesso e sua data de expiração.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginClientOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginClientInput input, [FromServices] ILoginClientUseCase useCase)
    {
        var result = await useCase.Execute(input);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        // CORREÇÃO: Padroniza a resposta de erro.
        return BadRequest(new { errors = result.Error?.GetErrors() });
    }
    
    /// <summary>
    /// Obtém os dados do perfil do cliente autenticado.
    /// </summary>
    /// <param name="useCase">O caso de uso para obter o perfil.</param>
    /// <returns>Os dados do perfil do cliente.</returns>
    [Authorize] // Este atributo protege o endpoint!
    [HttpGet("me")]
    [ProducesResponseType(typeof(CreateClientOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMyProfile([FromServices] IGetMyProfileUseCase useCase)
    {
        // O input é nulo porque o ID do usuário é obtido do token JWT, e não de um input explícito.
        var result = await useCase.Execute(null);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        // CORREÇÃO: Padroniza a resposta de erro.
        return BadRequest(new { errors = result.Error?.GetErrors() });
    }
    
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(LoginClientOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenInput input, [FromServices] IRefreshTokenUseCase useCase)
    {
        var result = await useCase.Execute(input);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(new { errors = result.Error?.GetErrors() });
    }
}
