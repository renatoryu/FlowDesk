namespace FlowDesk.Application.Attachments.List;

public sealed record AttachmentListItem(
    Guid Id,
    Guid TicketId,
    Guid UploadedById,
    string OriginalFileName,
    string ContentType,
    long SizeInBytes,
    DateTime CreatedAtUtc);
