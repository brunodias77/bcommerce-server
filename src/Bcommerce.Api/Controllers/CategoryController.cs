using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bcomerce.Application.UseCases.Catalog.Common;
using Bcomerce.Application.UseCases.Catalog.CreateCategory;
using Bcomerce.Application.UseCases.Catalog.DeleteCategories;
using Bcomerce.Application.UseCases.Catalog.ListCategories;
using Bcomerce.Application.UseCases.Catalog.UpdateCategory;
using Bcommerce.Domain.Validations;
using Microsoft.AspNetCore.Mvc;

namespace Bcommerce.Api.Controllers
{
    [ApiController]
    [Route("api/categories")]
    //[Authorize(Roles = "Administrator")] // Descomente quando o sistema de Roles estiver pronto
    public class CategoryController : ControllerBase
    {

        [HttpPost]
        [ProducesResponseType(typeof(Bcomerce.Application.UseCases.Catalog.Common.CategoryOutput), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(List<Error>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
       [FromBody] CreateCategoryInput input,
       [FromServices] ICreateCategoryUseCase useCase)
        {
            var result = await useCase.Execute(input);

            if (result.IsSuccess)
            {
                // Idealmente, retornaria um link para o novo recurso
                return StatusCode(StatusCodes.Status201Created, result.Value);
            }

            return BadRequest(result.Error?.GetErrors());
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoryOutput>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromServices] IListCategoriesUseCase useCase)
        {
            var result = await useCase.Execute(null);
            return Ok(result.Value);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(CategoryOutput), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(List<Error>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(
                                                    [FromRoute] Guid id,
                                                    [FromBody] UpdateCategoryInput payload,
                                                    [FromServices] IUpdateCategoryUseCase useCase)
        {
            var input = payload with { Id = id };
            var result = await useCase.Execute(input);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error?.GetErrors());
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(List<Error>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, [FromServices] IDeleteCategoryUseCase useCase)
        {
            var result = await useCase.Execute(id);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error?.GetErrors());
        }
    }
}
