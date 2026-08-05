using FlowDesk.Domain.Entities;

namespace FlowDesk.UnitTests.Domain.Entities;

public sealed class CompanyTests
{

    private const string ValidTaxId =
        "12.345.678/0001-95";

    private const string NormalizedTaxId =
        "12345678000195";

    private const string ValidContactEmail =
        "contact@flowdesk.com.br";

    [Fact]
    public void ConstructorWithValidDataCreatesActiveCompany()
    {
        DateTime beforeCreation = DateTime.UtcNow;

        var company = new Company(
            "  FlowDesk Tecnologia  ",
            ValidTaxId,
            "  CONTACT@FLOWDESK.COM.BR  ");

        DateTime afterCreation = DateTime.UtcNow;

        Assert.NotEqual(Guid.Empty, company.Id);
        Assert.Equal(
            "FlowDesk Tecnologia",
            company.Name);
        Assert.Equal(
            NormalizedTaxId,
            company.TaxId);
        Assert.Equal(
            ValidContactEmail,
            company.ContactEmail);
        Assert.True(company.IsActive);
        Assert.InRange(
            company.CreatedAtUtc,
            beforeCreation,
            afterCreation);
        Assert.Equal(
            company.CreatedAtUtc,
            company.UpdatedAtUtc);
    }

    [Theory]
    [InlineData("12.345.678/0001-94")]
    [InlineData("11.111.111/1111-11")]
    [InlineData("123")]
    [InlineData("12.345.678/0001-9A")]
    [InlineData("١٢.٣٤٥.٦٧٨/٠٠٠١-95")]
    public void ConstructorWithInvalidTaxIdThrowsArgumentException(
        string invalidTaxId)
    {
        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new Company(
                    "FlowDesk Tecnologia",
                    invalidTaxId,
                    ValidContactEmail));

        Assert.Equal("taxId", exception.ParamName);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("contact@")]
    [InlineData("@flowdesk.com.br")]
    [InlineData("invalid email@example.com")]
    public void ConstructorWithInvalidEmailThrowsArgumentException(
        string invalidEmail)
    {
        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new Company(
                    "FlowDesk Tecnologia",
                    ValidTaxId,
                    invalidEmail));

        Assert.Equal(
            "contactEmail",
            exception.ParamName);
    }

    [Fact]
    public void ConstructorWithBlankNameThrowsArgumentException()
    {
        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new Company(
                    " ",
                    ValidTaxId,
                    ValidContactEmail));

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void ConstructorWithNameExceedingLimitThrowsArgumentException()
    {
        string longName =
            new('A', Company.MaxNameLength + 1);

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new Company(
                    longName,
                    ValidTaxId,
                    ValidContactEmail));

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void UpdateDetailsWithValidDataNormalizesValues()
    {
        Company company = CreateCompany();

        DateTime previousUpdatedAt =
            company.UpdatedAtUtc;

        company.UpdateDetails(
            "  FlowDesk Serviços  ",
            "  SUPPORT@FLOWDESK.COM.BR  ");

        Assert.Equal(
            "FlowDesk Serviços",
            company.Name);
        Assert.Equal(
            "support@flowdesk.com.br",
            company.ContactEmail);
        Assert.True(
            company.UpdatedAtUtc >= previousUpdatedAt);
    }

    [Fact]
    public void UpdateDetailsWithInvalidEmailDoesNotChangeValues()
    {
        Company company = CreateCompany();

        string originalName = company.Name;
        string originalEmail = company.ContactEmail;
        DateTime originalUpdatedAt =
            company.UpdatedAtUtc;

        Assert.Throws<ArgumentException>(
            () => company.UpdateDetails(
                "Changed Company",
                "invalid-email"));

        Assert.Equal(originalName, company.Name);
        Assert.Equal(
            originalEmail,
            company.ContactEmail);
        Assert.Equal(
            originalUpdatedAt,
            company.UpdatedAtUtc);
    }

    [Fact]
    public void ActivateAndDeactivateChangeCompanyStatus()
    {
        Company company = CreateCompany();

        company.Deactivate();

        Assert.False(company.IsActive);

        DateTime deactivatedAt =
            company.UpdatedAtUtc;

        company.Deactivate();

        Assert.Equal(
            deactivatedAt,
            company.UpdatedAtUtc);

        company.Activate();

        Assert.True(company.IsActive);
    }

    private static Company CreateCompany()
    {
        return new Company(
            "FlowDesk Tecnologia",
            ValidTaxId,
            ValidContactEmail);
    }
}
