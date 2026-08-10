using FlowDesk.Api.Authorization;
using FlowDesk.Api.Contracts.Tickets;
using FlowDesk.Application.Tickets.Create;
using FlowDesk.Application.Tickets.GetById;
using FlowDesk.Application.Tickets.List;
using FlowDesk.Application.Tickets.Update;
using FlowDesk.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowDesk.Api.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public sealed class TicketsController : ControllerBase
{
    private readonly CreateTicketHandler _createTicketHandler;
    private readonly GetTicketByIdHandler _getTicketByIdHandler;
    private readonly ListTicketsHandler _listTicketsHandler;
    private readonly UpdateTicketHandler _updateTicketHandler;

    public TicketsController(
        CreateTicketHandler createTicketHandler,
        GetTicketByIdHandler getTicketByIdHandler,
        ListTicketsHandler listTicketsHandler,
        UpdateTicketHandler updateTicketHandler)
    {
        _createTicketHandler = createTicketHandler;
        _getTicketByIdHandler = getTicketByIdHandler;
        _listTicketsHandler = listTicketsHandler;
        _updateTicketHandler = updateTicketHandler;
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.TicketCreate)]
    [ProducesResponseType<CreateTicketResult>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateTicketResult>> Create(
        CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTicketCommand(
            request.CategoryId,
            request.Title,
            request.Description,
            request.Priority);

        CreateTicketResult result =
            await _createTicketHandler.HandleAsync(
                command,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.TicketRead)]
    [ProducesResponseType<ListTicketsResult>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ListTicketsResult>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] TicketStatus? status = null,
        [FromQuery] TicketPriority? priority = null,
        [FromQuery] Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new ListTicketsQuery(
            page,
            pageSize,
            status,
            priority,
            categoryId);

        ListTicketsResult result =
            await _listTicketsHandler.HandleAsync(
                query,
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.TicketRead)]
    [ProducesResponseType<GetTicketByIdResult>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetTicketByIdResult>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        GetTicketByIdResult result =
            await _getTicketByIdHandler.HandleAsync(
                new GetTicketByIdQuery(id),
                cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.TicketUpdate)]
    [ProducesResponseType<UpdateTicketResult>(
    StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
    StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
    StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
    StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(
    StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(
    StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(
    StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UpdateTicketResult>> Update(
    Guid id,
    UpdateTicketRequest request,
    CancellationToken cancellationToken)
    {
        var command = new UpdateTicketCommand(
            id,
            request.CategoryId,
            request.Title,
            request.Description,
            request.Priority);

        UpdateTicketResult result =
            await _updateTicketHandler.HandleAsync(
                command,
                cancellationToken);

        return Ok(result);
    }

}
