using FlowDesk.Api.Contracts.Companies;
using FlowDesk.Application.Companies.Create;
using FlowDesk.Application.Companies.Deactivate;
using FlowDesk.Application.Companies.GetById;
using FlowDesk.Application.Companies.List;
using FlowDesk.Application.Companies.Update;
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
    private readonly UpdateCompanyHandler _updateCompanyHandler;
    private readonly DeactivateCompanyHandler _deactivateCompanyHandler;

    public CompaniesController(
        CreateCompanyHandler createCompanyHandler,
        GetCompanyByIdHandler getCompanyByIdHandler,
        ListCompaniesHandler listCompaniesHandler,
        UpdateCompanyHandler updateCompanyHandler,
        DeactivateCompanyHandler deactivateCompanyHandler)
    {
        _createCompanyHandler = createCompanyHandler;
        _getCompanyByIdHandler = getCompanyByIdHandler;
        _listCompaniesHandler = listCompaniesHandler;
        _updateCompanyHandler = updateCompanyHandler;
        _deactivateCompanyHandler = deactivateCompanyHandler;
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

    [HttpPut("{id:guid}")]
    [ProducesResponseType<UpdateCompanyResult>(
    StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
    StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
    StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(
    StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(
    StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(
    StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UpdateCompanyResult>> Update(
    Guid id,
    UpdateCompanyRequest request,
    CancellationToken cancellationToken)
    {
        var command = new UpdateCompanyCommand(
            id,
            request.Name,
            request.ContactEmail);

        UpdateCompanyResult result =
            await _updateCompanyHandler.HandleAsync(
                command,
                cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(
    StatusCodes.Status204NoContent)]
    [ProducesResponseType(
    StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(
    StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(
    StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(
    StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(
    Guid id,
    CancellationToken cancellationToken)
    {
        await _deactivateCompanyHandler.HandleAsync(
            new DeactivateCompanyCommand(id),
            cancellationToken);

        return NoContent();
    }


}
