using FlowDesk.Application.Comments.List;
using FluentValidation.Results;

namespace FlowDesk.UnitTests.Application.Comments.List;

public sealed class ListTicketCommentsQueryValidatorTests
{
    private readonly ListTicketCommentsQueryValidator _validator =
        new();

    [Fact]
    public void ValidateWithValidQuerySucceeds()
    {
        var query = new ListTicketCommentsQuery(
            Guid.NewGuid());

        ValidationResult result =
            _validator.Validate(query);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateWithEmptyTicketIdReturnsError()
    {
        var query = new ListTicketCommentsQuery(
            Guid.Empty);

        ValidationResult result =
            _validator.Validate(query);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(ListTicketCommentsQuery.TicketId));
    }
}
