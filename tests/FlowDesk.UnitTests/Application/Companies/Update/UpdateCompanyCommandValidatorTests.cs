using FlowDesk.Application.Companies.Update;
using FluentValidation.Results;

namespace FlowDesk.UnitTests.Application.Companies.Update;

public sealed class UpdateCompanyCommandValidatorTests
{
    private readonly UpdateCompanyCommandValidator _validator =
        new();

    [Fact]
    public void ValidateWithValidCommandSucceeds()
    {
        var command = new UpdateCompanyCommand(
            Guid.NewGuid(),
            "FlowDesk Support",
            "support@flowdesk.com.br");

        ValidationResult result =
            _validator.Validate(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateWithInvalidFieldsReturnsErrors()
    {
        var command = new UpdateCompanyCommand(
            Guid.Empty,
            string.Empty,
            "invalid-email");

        ValidationResult result =
            _validator.Validate(command);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(UpdateCompanyCommand.Id));

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(UpdateCompanyCommand.Name));

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(UpdateCompanyCommand.ContactEmail));
    }
}
