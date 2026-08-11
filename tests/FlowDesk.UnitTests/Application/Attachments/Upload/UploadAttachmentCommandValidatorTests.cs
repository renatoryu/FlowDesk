using System.Text;
using FlowDesk.Application.Attachments.Upload;
using FlowDesk.Domain.Entities;
using FluentValidation.Results;

namespace FlowDesk.UnitTests.Application.Attachments.Upload;

public sealed class UploadAttachmentCommandValidatorTests
{
    private readonly UploadAttachmentCommandValidator _validator =
        new();

    [Fact]
    public async Task ValidateWithValidPdfSucceeds()
    {
        ValidationResult result =
            await ValidateAsync(
                "evidence.pdf",
                Attachment.PdfContentType,
                CreatePdfContent());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateWithValidPngSucceeds()
    {
        ValidationResult result =
            await ValidateAsync(
                "evidence.png",
                Attachment.PngContentType,
                CreatePngContent());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateWithValidJpegSucceeds()
    {
        ValidationResult result =
            await ValidateAsync(
                "evidence.jpg",
                Attachment.JpegContentType,
                CreateJpegContent());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateWithMismatchedContentTypeReturnsError()
    {
        ValidationResult result =
            await ValidateAsync(
                "evidence.png",
                Attachment.PdfContentType,
                CreatePdfContent());

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(UploadAttachmentCommand.ContentType));
    }

    [Fact]
    public async Task ValidateWithInvalidSignatureReturnsError()
    {
        byte[] content =
            Encoding.UTF8.GetBytes(
                "This is not a PDF file.");

        ValidationResult result =
            await ValidateAsync(
                "evidence.pdf",
                Attachment.PdfContentType,
                content);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(UploadAttachmentCommand.Content));
    }

    [Fact]
    public async Task ValidateWithUnsupportedExtensionReturnsError()
    {
        byte[] content =
            Encoding.UTF8.GetBytes(
                "Plain text content.");

        ValidationResult result =
            await ValidateAsync(
                "evidence.txt",
                "text/plain",
                content);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(UploadAttachmentCommand.OriginalFileName));
    }

    [Fact]
    public async Task ValidateWithDeclaredSizeDifferentFromContentReturnsError()
    {
        byte[] content = CreatePdfContent();

        ValidationResult result =
            await ValidateAsync(
                "evidence.pdf",
                Attachment.PdfContentType,
                content,
                content.LongLength + 1);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(UploadAttachmentCommand.SizeInBytes));
    }

    [Fact]
    public async Task ValidateWithSizeExceedingLimitReturnsError()
    {
        ValidationResult result =
            await ValidateAsync(
                "evidence.pdf",
                Attachment.PdfContentType,
                CreatePdfContent(),
                Attachment.MaxFileSizeInBytes + 1);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(UploadAttachmentCommand.SizeInBytes));
    }

    [Fact]
    public async Task ValidateWithUnsafeFileNameReturnsError()
    {
        ValidationResult result =
            await ValidateAsync(
                "../evidence.pdf",
                Attachment.PdfContentType,
                CreatePdfContent());

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(UploadAttachmentCommand.OriginalFileName));
    }

    private async Task<ValidationResult> ValidateAsync(
        string fileName,
        string contentType,
        byte[] content,
        long? declaredSize = null)
    {
        using var stream =
            new MemoryStream(
                content,
                writable: false);

        var command = new UploadAttachmentCommand(
            Guid.NewGuid(),
            fileName,
            contentType,
            declaredSize ?? content.LongLength,
            stream);

        ValidationResult result =
            await _validator.ValidateAsync(command);

        Assert.Equal(0, stream.Position);

        return result;
    }

    private static byte[] CreatePdfContent()
    {
        return Encoding.ASCII.GetBytes(
            "%PDF-1.7");
    }

    private static byte[] CreatePngContent()
    {
        return
        [
            0x89,
            0x50,
            0x4E,
            0x47,
            0x0D,
            0x0A,
            0x1A,
            0x0A
        ];
    }

    private static byte[] CreateJpegContent()
    {
        return
        [
            0xFF,
            0xD8,
            0xFF,
            0xE0
        ];
    }
}
