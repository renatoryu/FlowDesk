using FlowDesk.Domain.Entities;

namespace FlowDesk.UnitTests.Domain.Entities;

public sealed class CommentTests
{
    [Fact]
    public void ConstructorWithValidDataCreatesNormalizedComment()
    {
        Guid ticketId = Guid.NewGuid();
        Guid authorId = Guid.NewGuid();

        var comment = new Comment(
            ticketId,
            authorId,
            "  I need help with this issue.  ");

        Assert.NotEqual(Guid.Empty, comment.Id);
        Assert.Equal(ticketId, comment.TicketId);
        Assert.Equal(authorId, comment.AuthorId);
        Assert.Equal(
            "I need help with this issue.",
            comment.Content);
        Assert.Equal(
            comment.CreatedAtUtc,
            comment.UpdatedAtUtc);
    }

    [Theory]
    [InlineData("ticketId")]
    [InlineData("authorId")]
    public void ConstructorWithEmptyRequiredIdThrowsArgumentException(
        string parameterName)
    {
        Guid ticketId = Guid.NewGuid();
        Guid authorId = Guid.NewGuid();

        if (parameterName == "ticketId")
        {
            ticketId = Guid.Empty;
        }
        else
        {
            authorId = Guid.Empty;
        }

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new Comment(
                    ticketId,
                    authorId,
                    "Comment content."));

        Assert.Equal(parameterName, exception.ParamName);
    }

    [Fact]
    public void ConstructorWithBlankContentThrowsArgumentException()
    {
        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new Comment(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    " "));

        Assert.Equal("content", exception.ParamName);
    }

    [Fact]
    public void ConstructorWithContentExceedingLimitThrowsArgumentException()
    {
        string content =
            new('A', Comment.MaxContentLength + 1);

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new Comment(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    content));

        Assert.Equal("content", exception.ParamName);
    }
}
