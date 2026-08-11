using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.Application.Attachments.Upload;

public sealed class UploadAttachmentCommandValidator
    : AbstractValidator<UploadAttachmentCommand>
{
    public UploadAttachmentCommandValidator()
    {
        RuleFor(command => command.TicketId)
            .NotEmpty();

        RuleFor(command => command.OriginalFileName)
            .NotEmpty()
            .MaximumLength(
                Attachment.MaxOriginalFileNameLength)
            .Must(IsSafeFileName)
            .WithMessage(
                "File name cannot contain a directory path.");

        RuleFor(command => command.ContentType)
            .NotEmpty()
            .MaximumLength(
                Attachment.MaxContentTypeLength)
            .Must(IsSupportedContentType)
            .WithMessage(
                "Only PDF, PNG and JPEG files are supported.");

        RuleFor(command => command.SizeInBytes)
            .InclusiveBetween(
                1,
                Attachment.MaxFileSizeInBytes);

        RuleFor(command => command.Content)
            .NotNull()
            .Must(IsReadableAndSeekable)
            .WithMessage(
                "File content must be readable and seekable.");

        RuleFor(command => command)
            .CustomAsync(ValidateFileAsync);
    }

    private static bool IsSafeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        string normalized = fileName.Trim();

        return normalized is not "." and not ".." &&
               !normalized.Contains('/') &&
               !normalized.Contains('\\') &&
               string.Equals(
                   normalized,
                   Path.GetFileName(normalized),
                   StringComparison.Ordinal);
    }

    private static bool IsSupportedContentType(
        string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        string normalized =
            contentType.Trim().ToLowerInvariant();

        return normalized is
            Attachment.PdfContentType or
            Attachment.PngContentType or
            Attachment.JpegContentType;
    }

    private static bool IsReadableAndSeekable(
        Stream? content)
    {
        return content is
        {
            CanRead: true,
            CanSeek: true
        };
    }

    private static async Task ValidateFileAsync(
        UploadAttachmentCommand command,
        ValidationContext<UploadAttachmentCommand> context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(
                command.OriginalFileName) ||
            string.IsNullOrWhiteSpace(
                command.ContentType) ||
            command.Content is not
            {
                CanRead: true,
                CanSeek: true
            })
        {
            return;
        }

        string extension =
            Path.GetExtension(
                    command.OriginalFileName.Trim())
                .ToLowerInvariant();

        string normalizedContentType =
            command.ContentType
                .Trim()
                .ToLowerInvariant();

        string? expectedContentType =
            extension switch
            {
                ".pdf" =>
                    Attachment.PdfContentType,

                ".png" =>
                    Attachment.PngContentType,

                ".jpg" or ".jpeg" =>
                    Attachment.JpegContentType,

                _ => null
            };

        if (expectedContentType is null)
        {
            context.AddFailure(
                nameof(command.OriginalFileName),
                "Only .pdf, .png, .jpg and .jpeg extensions are supported.");

            return;
        }

        if (!string.Equals(
                expectedContentType,
                normalizedContentType,
                StringComparison.Ordinal))
        {
            context.AddFailure(
                nameof(command.ContentType),
                "The content type does not match the file extension.");

            return;
        }

        if (command.Content.Length !=
            command.SizeInBytes)
        {
            context.AddFailure(
                nameof(command.SizeInBytes),
                "The declared file size does not match the content.");

            return;
        }

        byte[] signature = new byte[8];
        int bytesRead = 0;

        try
        {
            command.Content.Position = 0;

            while (bytesRead < signature.Length)
            {
                int currentRead =
                    await command.Content.ReadAsync(
                        signature.AsMemory(
                            bytesRead,
                            signature.Length - bytesRead),
                        cancellationToken);

                if (currentRead == 0)
                {
                    break;
                }

                bytesRead += currentRead;
            }
        }
        finally
        {
            command.Content.Position = 0;
        }

        if (!MatchesSignature(
                normalizedContentType,
                signature,
                bytesRead))
        {
            context.AddFailure(
                nameof(command.Content),
                "The file signature does not match its declared type.");
        }
    }

    private static bool MatchesSignature(
        string contentType,
        byte[] signature,
        int bytesRead)
    {
        return contentType switch
        {
            Attachment.PdfContentType =>
                bytesRead >= 5 &&
                signature[0] == 0x25 &&
                signature[1] == 0x50 &&
                signature[2] == 0x44 &&
                signature[3] == 0x46 &&
                signature[4] == 0x2D,

            Attachment.PngContentType =>
                bytesRead >= 8 &&
                signature[0] == 0x89 &&
                signature[1] == 0x50 &&
                signature[2] == 0x4E &&
                signature[3] == 0x47 &&
                signature[4] == 0x0D &&
                signature[5] == 0x0A &&
                signature[6] == 0x1A &&
                signature[7] == 0x0A,

            Attachment.JpegContentType =>
                bytesRead >= 3 &&
                signature[0] == 0xFF &&
                signature[1] == 0xD8 &&
                signature[2] == 0xFF,

            _ => false
        };
    }
}
