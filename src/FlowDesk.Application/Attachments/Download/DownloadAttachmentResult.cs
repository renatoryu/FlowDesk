namespace FlowDesk.Application.Attachments.Download;

public sealed record DownloadAttachmentResult(
    Stream Content,
    string ContentType,
    string FileName);
