namespace FlowDesk.Application.Attachments.Upload;

public sealed record UploadAttachmentResult(
    Guid Id,
    Guid TicketId,
    Guid UploadedById,
    string OriginalFileName,
    string ContentType,
    long SizeInBytes,
    DateTime CreatedAtUtc);
