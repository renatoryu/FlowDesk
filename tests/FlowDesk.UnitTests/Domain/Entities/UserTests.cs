using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;

namespace FlowDesk.UnitTests.Domain.Entities;

public sealed class UserTests
{
    private const string ValidPasswordHash = "hashed-password";

    [Fact]
    public void ConstructorWithValidDataCreatesActiveCustomer()
    {
        DateTime beforeCreation = DateTime.UtcNow;

        var user = new User(
            "  Ana Silva  ",
            "  ANA.SILVA@EXAMPLE.COM  ",
            ValidPasswordHash);

        DateTime afterCreation = DateTime.UtcNow;

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Ana Silva", user.FullName);
        Assert.Equal("ana.silva@example.com", user.Email);
        Assert.Equal(ValidPasswordHash, user.PasswordHash);
        Assert.Equal(UserRole.Customer, user.Role);
        Assert.True(user.IsActive);
        Assert.InRange(user.CreatedAtUtc, beforeCreation, afterCreation);
        Assert.Equal(user.CreatedAtUtc, user.UpdatedAtUtc);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("ana@")]
    [InlineData("@example.com")]
    public void ConstructorWithInvalidEmailThrowsArgumentException(
        string invalidEmail)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new User(
                "Ana Silva",
                invalidEmail,
                ValidPasswordHash));

        Assert.Equal("email", exception.ParamName);
    }

    [Fact]
    public void ConstructorWithBlankFullNameThrowsArgumentException()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new User(
                " ",
                "ana@example.com",
                ValidPasswordHash));

        Assert.Equal("fullName", exception.ParamName);
    }

    [Fact]
    public void ConstructorWithBlankPasswordHashThrowsArgumentException()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new User(
                "Ana Silva",
                "ana@example.com",
                " "));

        Assert.Equal("passwordHash", exception.ParamName);
    }

    [Fact]
    public void UpdateProfileWithValidDataNormalizesValues()
    {
        User user = CreateUser();

        user.UpdateProfile(
            "  Ana Souza  ",
            "  ANA.SOUZA@EXAMPLE.COM  ");

        Assert.Equal("Ana Souza", user.FullName);
        Assert.Equal("ana.souza@example.com", user.Email);
    }

    [Fact]
    public void ChangePasswordHashWithValidHashUpdatesPasswordHash()
    {
        User user = CreateUser();

        user.ChangePasswordHash("new-password-hash");

        Assert.Equal("new-password-hash", user.PasswordHash);
    }

    [Fact]
    public void ChangeRoleWithValidRoleUpdatesRole()
    {
        User user = CreateUser();

        user.ChangeRole(UserRole.Admin);

        Assert.Equal(UserRole.Admin, user.Role);
    }

    [Fact]
    public void ChangeRoleWithUndefinedRoleThrowsArgumentOutOfRangeException()
    {
        User user = CreateUser();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => user.ChangeRole((UserRole)999));
    }

    [Fact]
    public void ActivateAndDeactivateChangeUserStatus()
    {
        User user = CreateUser();

        user.Deactivate();

        Assert.False(user.IsActive);

        user.Activate();

        Assert.True(user.IsActive);
    }

    private static User CreateUser()
    {
        return new User(
            "Ana Silva",
            "ana@example.com",
            ValidPasswordHash);
    }
}
