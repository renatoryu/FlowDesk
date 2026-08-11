namespace FlowDesk.Application.Attachments.List;

public sealed record ListTicketAttachmentsResult(
    IReadOnlyList<AttachmentListItem> Items);
