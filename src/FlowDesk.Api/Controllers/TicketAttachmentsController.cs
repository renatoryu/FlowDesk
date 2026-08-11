using FlowDesk.Api.Authorization;
using FlowDesk.Application.Attachments.Download;
using FlowDesk.Application.Attachments.List;
using FlowDesk.Application.Attachments.Upload;
using FlowDesk.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowDesk.Api.Controllers;

[ApiController]
[Route("api/tickets/{ticketId:guid}/attachments")]
[Authorize]
public sealed class TicketAttachmentsController : ControllerBase
{
    private readonly UploadAttachmentHandler _uploadAttachmentHandler;

    private readonly ListTicketAttachmentsHandler _listTicketAttachmentsHandler;

    private readonly DownloadAttachmentHandler _downloadAttachmentHandler;

    public TicketAttachmentsController(
        UploadAttachmentHandler uploadAttachmentHandler,
        ListTicketAttachmentsHandler listTicketAttachmentsHandler,
        DownloadAttachmentHandler downloadAttachmentHandler)
    {
        _uploadAttachmentHandler = uploadAttachmentHandler;
        _listTicketAttachmentsHandler =
            listTicketAttachmentsHandler;
        _downloadAttachmentHandler = downloadAttachmentHandler;
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AttachmentUpload)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(
        Attachment.MaxFileSizeInBytes + 1_048_576)]
    [ProducesResponseType<UploadAttachmentResult>(
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
    [ProducesResponseType(
        StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UploadAttachmentResult>> Upload(
        Guid ticketId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using Stream content =
            file.OpenReadStream();

        UploadAttachmentResult result =
            await _uploadAttachmentHandler.HandleAsync(
                new UploadAttachmentCommand(
                    ticketId,
                    file.FileName,
                    file.ContentType,
                    file.Length,
                    content),
                cancellationToken);

        return CreatedAtAction(
            nameof(List),
            new { ticketId },
            result);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AttachmentRead)]
    [ProducesResponseType<ListTicketAttachmentsResult>(
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
    public async Task<ActionResult<ListTicketAttachmentsResult>> List(
    Guid ticketId,
    CancellationToken cancellationToken)
    {
        ListTicketAttachmentsResult result =
            await _listTicketAttachmentsHandler.HandleAsync(
                new ListTicketAttachmentsQuery(ticketId),
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("{attachmentId:guid}/download")]
    [Authorize(Policy = AuthorizationPolicies.AttachmentRead)]
    [ProducesResponseType(
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
    public async Task<IActionResult> Download(
    Guid ticketId,
    Guid attachmentId,
    CancellationToken cancellationToken)
    {
        DownloadAttachmentResult result =
            await _downloadAttachmentHandler.HandleAsync(
                new DownloadAttachmentQuery(
                    ticketId,
                    attachmentId),
                cancellationToken);

        return File(
            result.Content,
            result.ContentType,
            result.FileName,
            enableRangeProcessing: true);
    }

}
