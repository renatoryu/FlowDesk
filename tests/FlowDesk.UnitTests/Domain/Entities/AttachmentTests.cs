using FlowDesk.Domain.Entities;

namespace FlowDesk.UnitTests.Domain.Entities;

public sealed class AttachmentTests
{
    [Fact]
    public void ConstructorWithValidDataCreatesNormalizedAttachment()
    {
        Guid ticketId = Guid.NewGuid();
        Guid uploadedById = Guid.NewGuid();

        var attachment = new Attachment(
            ticketId,
            uploadedById,
            "  evidence.pdf  ",
            "  stored-file.pdf  ",
            "  APPLICATION/PDF  ",
            1024);

        Assert.NotEqual(Guid.Empty, attachment.Id);
        Assert.Equal(ticketId, attachment.TicketId);
        Assert.Equal(uploadedById, attachment.UploadedById);
        Assert.Equal(
            "evidence.pdf",
            attachment.OriginalFileName);
        Assert.Equal(
            "stored-file.pdf",
            attachment.StoredFileName);
        Assert.Equal(
            Attachment.PdfContentType,
            attachment.ContentType);
        Assert.Equal(1024, attachment.SizeInBytes);
        Assert.Equal(
            attachment.CreatedAtUtc,
            attachment.UpdatedAtUtc);
    }

    [Theory]
    [InlineData(Attachment.PdfContentType)]
    [InlineData(Attachment.PngContentType)]
    [InlineData(Attachment.JpegContentType)]
    public void ConstructorWithSupportedContentTypeAcceptsValue(
        string contentType)
    {
        var attachment = new Attachment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "evidence.bin",
            "stored-file.bin",
            contentType,
            1);

        Assert.Equal(contentType, attachment.ContentType);
    }

    [Theory]
    [InlineData("ticketId")]
    [InlineData("uploadedById")]
    public void ConstructorWithEmptyRequiredIdThrowsArgumentException(
        string parameterName)
    {
        Guid ticketId = Guid.NewGuid();
        Guid uploadedById = Guid.NewGuid();

        if (parameterName == "ticketId")
        {
            ticketId = Guid.Empty;
        }
        else
        {
            uploadedById = Guid.Empty;
        }

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new Attachment(
                    ticketId,
                    uploadedById,
                    "evidence.pdf",
                    "stored-file.pdf",
                    Attachment.PdfContentType,
                    1));

        Assert.Equal(parameterName, exception.ParamName);
    }

    [Theory]
    [InlineData("../evidence.pdf")]
    [InlineData("folder/evidence.pdf")]
    [InlineData(@"C:\evidence.pdf")]
    [InlineData(".")]
    [InlineData("..")]
    public void ConstructorWithUnsafeOriginalFileNameThrowsArgumentException(
        string fileName)
    {
        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new Attachment(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    fileName,
                    "stored-file.pdf",
                    Attachment.PdfContentType,
                    1));

        Assert.Equal(
            "originalFileName",
            exception.ParamName);
    }

    [Fact]
    public void ConstructorWithOriginalFileNameExceedingLimitThrowsArgumentException()
    {
        string fileName =
            new(
                'A',
                Attachment.MaxOriginalFileNameLength + 1);

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new Attachment(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    fileName,
                    "stored-file.pdf",
                    Attachment.PdfContentType,
                    1));

        Assert.Equal(
            "originalFileName",
            exception.ParamName);
    }

    [Fact]
    public void ConstructorWithStoredFileNameExceedingLimitThrowsArgumentException()
    {
        string fileName =
            new(
                'A',
                Attachment.MaxStoredFileNameLength + 1);

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new Attachment(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "evidence.pdf",
                    fileName,
                    Attachment.PdfContentType,
                    1));

        Assert.Equal(
            "storedFileName",
            exception.ParamName);
    }

    [Theory]
    [InlineData("text/plain")]
    [InlineData("image/gif")]
    [InlineData(" ")]
    public void ConstructorWithUnsupportedContentTypeThrowsArgumentException(
        string contentType)
    {
        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new Attachment(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "evidence.pdf",
                    "stored-file.pdf",
                    contentType,
                    1));

        Assert.Equal("contentType", exception.ParamName);
    }

    [Theory]
    [InlineData(-1L)]
    [InlineData(0L)]
    [InlineData(Attachment.MaxFileSizeInBytes + 1)]
    public void ConstructorWithInvalidSizeThrowsArgumentOutOfRangeException(
        long sizeInBytes)
    {
        ArgumentOutOfRangeException exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Attachment(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "evidence.pdf",
                    "stored-file.pdf",
                    Attachment.PdfContentType,
                    sizeInBytes));

        Assert.Equal("sizeInBytes", exception.ParamName);
    }
}
