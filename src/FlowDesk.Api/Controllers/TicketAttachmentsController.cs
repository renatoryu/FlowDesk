using FlowDesk.Api.Authorization;
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

    public TicketAttachmentsController(
        UploadAttachmentHandler uploadAttachmentHandler)
    {
        _uploadAttachmentHandler = uploadAttachmentHandler;
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

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }
}
