using FlowDesk.Application.Categories.List;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowDesk.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public sealed class CategoriesController : ControllerBase
{
    private readonly ListCategoriesHandler _listCategoriesHandler;

    public CategoriesController(
        ListCategoriesHandler listCategoriesHandler)
    {
        _listCategoriesHandler = listCategoriesHandler;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ListCategoryResult>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ListCategoryResult>>> List(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ListCategoryResult> result =
            await _listCategoriesHandler.HandleAsync(
                new ListCategoriesQuery(),
                cancellationToken);

        return Ok(result);
    }
}
