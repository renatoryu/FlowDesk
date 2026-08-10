using FlowDesk.Api.Authorization;
using FlowDesk.Api.Contracts.Comments;
using FlowDesk.Application.Comments.Create;
using FlowDesk.Application.Comments.List;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowDesk.Api.Controllers;

[ApiController]
[Route("api/tickets/{ticketId:guid}/comments")]
[Authorize]
public sealed class TicketCommentsController : ControllerBase
{
    private readonly CreateCommentHandler _createCommentHandler;
    private readonly ListTicketCommentsHandler _listTicketCommentsHandler;

    public TicketCommentsController(
        CreateCommentHandler createCommentHandler,
        ListTicketCommentsHandler listTicketCommentsHandler)
    {
        _createCommentHandler = createCommentHandler;
        _listTicketCommentsHandler = listTicketCommentsHandler;
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CommentCreate)]
    [ProducesResponseType<CreateCommentResult>(
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
    public async Task<ActionResult<CreateCommentResult>> Create(
        Guid ticketId,
        CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        CreateCommentResult result =
            await _createCommentHandler.HandleAsync(
                new CreateCommentCommand(
                    ticketId,
                    request.Content),
                cancellationToken);

        return CreatedAtAction(
            nameof(List),
            new { ticketId },
            result);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.CommentRead)]
    [ProducesResponseType<ListTicketCommentsResult>(
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
    public async Task<ActionResult<ListTicketCommentsResult>> List(
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        ListTicketCommentsResult result =
            await _listTicketCommentsHandler.HandleAsync(
                new ListTicketCommentsQuery(ticketId),
                cancellationToken);

        return Ok(result);
    }
}
