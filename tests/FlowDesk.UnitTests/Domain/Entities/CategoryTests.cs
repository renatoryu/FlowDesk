using FlowDesk.Domain.Entities;

namespace FlowDesk.UnitTests.Domain.Entities;

public sealed class CategoryTests
{
    [Fact]
    public void ConstructorWithValidDataCreatesActiveCategory()
    {
        DateTime beforeCreation = DateTime.UtcNow;

        var category = new Category(
            "  Technical Support  ",
            "  Problems involving the application.  ");

        DateTime afterCreation = DateTime.UtcNow;

        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.Equal(
            "Technical Support",
            category.Name);
        Assert.Equal(
            "TECHNICAL SUPPORT",
            category.NormalizedName);
        Assert.Equal(
            "Problems involving the application.",
            category.Description);
        Assert.True(category.IsActive);
        Assert.InRange(
            category.CreatedAtUtc,
            beforeCreation,
            afterCreation);
        Assert.Equal(
            category.CreatedAtUtc,
            category.UpdatedAtUtc);
    }

    [Fact]
    public void ConstructorWithBlankDescriptionStoresNull()
    {
        var category = new Category(
            "General",
            "   ");

        Assert.Null(category.Description);
    }

    [Fact]
    public void ConstructorWithBlankNameThrowsArgumentException()
    {
        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new Category(" "));

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void ConstructorWithNameExceedingLimitThrowsArgumentException()
    {
        string longName =
            new('A', Category.MaxNameLength + 1);

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new Category(longName));

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void ConstructorWithDescriptionExceedingLimitThrowsArgumentException()
    {
        string longDescription =
            new('A', Category.MaxDescriptionLength + 1);

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new Category(
                    "General",
                    longDescription));

        Assert.Equal(
            "description",
            exception.ParamName);
    }

    [Fact]
    public void UpdateDetailsWithValidDataNormalizesValues()
    {
        Category category = CreateCategory();

        category.UpdateDetails(
            "  Customer Access  ",
            "  Login and permission problems.  ");

        Assert.Equal(
            "Customer Access",
            category.Name);
        Assert.Equal(
            "CUSTOMER ACCESS",
            category.NormalizedName);
        Assert.Equal(
            "Login and permission problems.",
            category.Description);
    }

    [Fact]
    public void UpdateDetailsWithInvalidDescriptionDoesNotChangeValues()
    {
        Category category = CreateCategory();

        string originalName = category.Name;
        string originalNormalizedName =
            category.NormalizedName;
        string? originalDescription =
            category.Description;
        DateTime originalUpdatedAt =
            category.UpdatedAtUtc;

        string longDescription =
            new('A', Category.MaxDescriptionLength + 1);

        Assert.Throws<ArgumentException>(
            () => category.UpdateDetails(
                "Changed Category",
                longDescription));

        Assert.Equal(originalName, category.Name);
        Assert.Equal(
            originalNormalizedName,
            category.NormalizedName);
        Assert.Equal(
            originalDescription,
            category.Description);
        Assert.Equal(
            originalUpdatedAt,
            category.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetailsWithSameDataDoesNotChangeTimestamp()
    {
        Category category = CreateCategory();

        DateTime originalUpdatedAt =
            category.UpdatedAtUtc;

        category.UpdateDetails(
            "  Technical Support  ",
            "  Application problems.  ");

        Assert.Equal(
            originalUpdatedAt,
            category.UpdatedAtUtc);
    }

    [Fact]
    public void ActivateAndDeactivateAreIdempotent()
    {
        Category category = CreateCategory();

        category.Deactivate();

        Assert.False(category.IsActive);

        DateTime deactivatedAt =
            category.UpdatedAtUtc;

        category.Deactivate();

        Assert.Equal(
            deactivatedAt,
            category.UpdatedAtUtc);

        category.Activate();

        Assert.True(category.IsActive);
    }

    private static Category CreateCategory()
    {
        return new Category(
            "Technical Support",
            "Application problems.");
    }
}
