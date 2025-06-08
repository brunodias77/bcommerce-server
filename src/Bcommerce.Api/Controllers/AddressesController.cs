using Bcomerce.Application.UseCases.Clients.AddAddress;
using Bcomerce.Application.UseCases.Clients.ListAddresses;
using Bcommerce.Domain.Validations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.Api.Controllers;


[ApiController]
[Route("api/addresses")]
[Authorize] // <<< IMPORTANTE: Protege todos os endpoints neste controller
public class AddressesController : ControllerBase
{
    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AddressOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyAddresses([FromServices] IListMyAddressesUseCase useCase)
    {
        var result = await useCase.Execute(null); // Não precisa de input

        // O 'Execute' sempre retornará sucesso, mesmo que a lista esteja vazia.
        // Erros aqui seriam exceções inesperadas.
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        // Este bloco seria para erros de validação, improvável neste caso de uso.
        return BadRequest(result.Error?.GetErrors());
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(AddressOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(List<Error>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddAddress(
        [FromBody] AddAddressInput input,
        [FromServices] IAddAddressUseCase useCase)
    {
        var result = await useCase.Execute(input);

        if (result.IsSuccess)
        {
            // Retorna 201 Created com o endereço recém-criado no corpo da resposta
            return StatusCode(StatusCodes.Status201Created, result.Value);
        }

        return BadRequest(result.Error?.GetErrors());
    }
    
    [HttpPut("{addressId:guid}")]
    [ProducesResponseType(typeof(AddressOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(List<Error>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAddress(
        [FromRoute] Guid addressId,
        [FromBody] object input, /* Crie um UpdateAddressInput aqui */
        [FromServices] object useCase /* Crie e injete um IUpdateAddressUseCase */)
    {
        // A implementação seguirá o mesmo padrão:
        // var result = await useCase.Execute(addressId, input);
        // if (result.IsSuccess) return Ok(result.Value);
        // return BadRequest(result.Error?.GetErrors());
        
        return Ok(); // Placeholder
    }
    
    [HttpDelete("{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(List<Error>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAddress(
        [FromRoute] Guid addressId,
        [FromServices] object useCase /* Crie e injete um IDeleteAddressUseCase */)
    {
        // A implementação seguirá o mesmo padrão:
        // var result = await useCase.Execute(addressId);
        // if (result.IsSuccess) return NoContent();
        // return BadRequest(result.Error?.GetErrors());
        
        return NoContent(); // Placeholder
    }
}