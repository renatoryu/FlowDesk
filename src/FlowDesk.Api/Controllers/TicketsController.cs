using FlowDesk.Api.Authorization;
using FlowDesk.Api.Contracts.Tickets;
using FlowDesk.Application.Tickets.Create;
using FlowDesk.Application.Tickets.List;
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
    private readonly ListTicketsHandler _listTicketsHandler;

    public TicketsController(
        CreateTicketHandler createTicketHandler,
        ListTicketsHandler listTicketsHandler)
    {
        _createTicketHandler = createTicketHandler;
        _listTicketsHandler = listTicketsHandler;
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

        return StatusCode(
            StatusCodes.Status201Created,
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
}
