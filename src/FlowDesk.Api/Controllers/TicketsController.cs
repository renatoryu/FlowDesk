using FlowDesk.Api.Authorization;
using FlowDesk.Api.Contracts.Tickets;
using FlowDesk.Application.Tickets.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowDesk.Api.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize(Policy = AuthorizationPolicies.TicketCreate)]
public sealed class TicketsController : ControllerBase
{
    private readonly CreateTicketHandler _createTicketHandler;

    public TicketsController(
        CreateTicketHandler createTicketHandler)
    {
        _createTicketHandler = createTicketHandler;
    }

    [HttpPost]
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
}
