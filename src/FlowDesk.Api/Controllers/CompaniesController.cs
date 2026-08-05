using FlowDesk.Application.Companies.Create;
using FlowDesk.Application.Companies.GetById;
using FlowDesk.Application.Companies.List;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace FlowDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/companies")]
public sealed class CompaniesController : ControllerBase
{
    private readonly CreateCompanyHandler _createCompanyHandler;
    private readonly GetCompanyByIdHandler _getCompanyByIdHandler;
    private readonly ListCompaniesHandler _listCompaniesHandler;

    public CompaniesController(
        CreateCompanyHandler createCompanyHandler,
        GetCompanyByIdHandler getCompanyByIdHandler,
        ListCompaniesHandler listCompaniesHandler)
    {
        _createCompanyHandler = createCompanyHandler;
        _getCompanyByIdHandler = getCompanyByIdHandler;
        _listCompaniesHandler = listCompaniesHandler;
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

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<GetCompanyByIdResult>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetCompanyByIdResult>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetCompanyByIdQuery(id);

        GetCompanyByIdResult result =
            await _getCompanyByIdHandler.HandleAsync(
                query,
                cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ListCompanyResult>>(
    StatusCodes.Status200OK)]
    [ProducesResponseType(
    StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(
    StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<ListCompanyResult>>> List(
    CancellationToken cancellationToken,
    [FromQuery] bool includeInactive = false)
    {
        var query =
            new ListCompaniesQuery(includeInactive);

        IReadOnlyList<ListCompanyResult> result =
            await _listCompaniesHandler.HandleAsync(
                query,
                cancellationToken);

        return Ok(result);
    }

}
