namespace FlowDesk.Application.Attachments.Upload;

public sealed record UploadAttachmentCommand(
    Guid TicketId,
    string OriginalFileName,
    string ContentType,
    long SizeInBytes,
    Stream Content);
