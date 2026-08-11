using FlowDesk.Domain.Common;

namespace FlowDesk.Domain.Entities;

public sealed class Attachment : BaseEntity
{
    public const int MaxOriginalFileNameLength = 255;
    public const int MaxStoredFileNameLength = 100;
    public const int MaxContentTypeLength = 100;
    public const long MaxFileSizeInBytes =
        10L * 1024 * 1024;

    public const string PdfContentType =
        "application/pdf";

    public const string PngContentType =
        "image/png";

    public const string JpegContentType =
        "image/jpeg";

    private Attachment()
    {
    }

    public Attachment(
        Guid ticketId,
        Guid uploadedById,
        string originalFileName,
        string storedFileName,
        string contentType,
        long sizeInBytes)
    {
        TicketId = ValidateRequiredId(
            ticketId,
            nameof(ticketId));

        UploadedById = ValidateRequiredId(
            uploadedById,
            nameof(uploadedById));

        OriginalFileName = NormalizeFileName(
            originalFileName,
            MaxOriginalFileNameLength,
            nameof(originalFileName));

        StoredFileName = NormalizeFileName(
            storedFileName,
            MaxStoredFileNameLength,
            nameof(storedFileName));

        ContentType = NormalizeContentType(contentType);
        SizeInBytes = ValidateSize(sizeInBytes);
    }

    public Guid TicketId { get; private set; }

    public Guid UploadedById { get; private set; }

    public string OriginalFileName { get; private set; } =
        string.Empty;

    public string StoredFileName { get; private set; } =
        string.Empty;

    public string ContentType { get; private set; } =
        string.Empty;

    public long SizeInBytes { get; private set; }

    private static Guid ValidateRequiredId(
        Guid id,
        string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Identifier cannot be empty.",
                parameterName);
        }

        return id;
    }

    private static string NormalizeFileName(
        string fileName,
        int maxLength,
        string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            fileName,
            parameterName);

        string normalized = fileName.Trim();

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException(
                $"File name cannot exceed {maxLength} characters.",
                parameterName);
        }

        if (normalized.Contains('/') ||
            normalized.Contains('\\') ||
            normalized is "." or "..")
        {
            throw new ArgumentException(
                "File name cannot contain directory paths.",
                parameterName);
        }

        return normalized;
    }

    private static string NormalizeContentType(
        string contentType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            contentType);

        string normalized =
            contentType.Trim().ToLowerInvariant();

        if (normalized != PdfContentType &&
            normalized != PngContentType &&
            normalized != JpegContentType)
        {
            throw new ArgumentException(
                "Unsupported attachment content type.",
                nameof(contentType));
        }

        return normalized;
    }

    private static long ValidateSize(long sizeInBytes)
    {
        if (sizeInBytes <= 0 ||
            sizeInBytes > MaxFileSizeInBytes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sizeInBytes),
                $"Attachment size must be between 1 and {MaxFileSizeInBytes} bytes.");
        }

        return sizeInBytes;
    }
}
