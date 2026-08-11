namespace FlowDesk.Application.Attachments.Download;

public sealed record DownloadAttachmentQuery(
    Guid TicketId,
    Guid AttachmentId);
