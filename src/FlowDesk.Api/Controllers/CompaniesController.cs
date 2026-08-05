using FlowDesk.Application.Companies.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/companies")]
public sealed class CompaniesController : ControllerBase
{
    private readonly CreateCompanyHandler _createCompanyHandler;

    public CompaniesController(
        CreateCompanyHandler createCompanyHandler)
    {
        _createCompanyHandler = createCompanyHandler;
    }

    [HttpPost]
    [ProducesResponseType<CreateCompanyResult>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateCompanyResult>> Create(
        CreateCompanyCommand command,
        CancellationToken cancellationToken)
    {
        CreateCompanyResult result =
            await _createCompanyHandler.HandleAsync(
                command,
                cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }
}
