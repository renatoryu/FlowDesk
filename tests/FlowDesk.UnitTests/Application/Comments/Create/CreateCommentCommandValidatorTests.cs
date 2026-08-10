using FlowDesk.Application.Comments.Create;
using FlowDesk.Domain.Entities;
using FluentValidation.Results;

namespace FlowDesk.UnitTests.Application.Comments.Create;

public sealed class CreateCommentCommandValidatorTests
{
    private readonly CreateCommentCommandValidator _validator =
        new();

    [Fact]
    public void ValidateWithValidCommandSucceeds()
    {
        var command = new CreateCommentCommand(
            Guid.NewGuid(),
            "I need help with this issue.");

        ValidationResult result =
            _validator.Validate(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateWithEmptyTicketIdReturnsError()
    {
        var command = new CreateCommentCommand(
            Guid.Empty,
            "I need help with this issue.");

        ValidationResult result =
            _validator.Validate(command);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(CreateCommentCommand.TicketId));
    }

    [Fact]
    public void ValidateWithBlankContentReturnsError()
    {
        var command = new CreateCommentCommand(
            Guid.NewGuid(),
            " ");

        ValidationResult result =
            _validator.Validate(command);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(CreateCommentCommand.Content));
    }

    [Fact]
    public void ValidateWithContentExceedingLimitReturnsError()
    {
        var command = new CreateCommentCommand(
            Guid.NewGuid(),
            new string(
                'A',
                Comment.MaxContentLength + 1));

        ValidationResult result =
            _validator.Validate(command);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(CreateCommentCommand.Content));
    }
}
