using FlowDesk.Application.Companies.Create;
using FluentValidation.Results;

namespace FlowDesk.UnitTests.Application.Companies.Create;

public sealed class CreateCompanyCommandValidatorTests
{
    private readonly CreateCompanyCommandValidator _validator =
        new();

    [Fact]
    public void ValidateWithValidCommandSucceeds()
    {
        var command = new CreateCompanyCommand(
            "FlowDesk Tecnologia",
            "12.345.678/0001-95",
            "contact@flowdesk.com.br");

        ValidationResult result =
            _validator.Validate(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateWithInvalidFieldsReturnsErrors()
    {
        var command = new CreateCompanyCommand(
            string.Empty,
            "123",
            "invalid email@example.com");

        ValidationResult result =
            _validator.Validate(command);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(CreateCompanyCommand.Name));

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(CreateCompanyCommand.TaxId));

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(CreateCompanyCommand.ContactEmail));
    }
}
