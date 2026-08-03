using FlowDesk.Domain.Entities;

namespace FlowDesk.UnitTests.Domain.Entities;

public sealed class RefreshTokenTests
{
    private const string ValidTokenHash = "valid-refresh-token-hash";

    [Fact]
    public void ConstructorWithValidDataCreatesActiveToken()
    {
        Guid userId = Guid.NewGuid();
        DateTime expiresAtUtc = DateTime.UtcNow.AddDays(7);

        var refreshToken = new RefreshToken(
            userId,
            $"  {ValidTokenHash}  ",
            expiresAtUtc);

        Assert.NotEqual(Guid.Empty, refreshToken.Id);
        Assert.Equal(userId, refreshToken.UserId);
        Assert.Equal(ValidTokenHash, refreshToken.TokenHash);
        Assert.Equal(expiresAtUtc, refreshToken.ExpiresAtUtc);
        Assert.Null(refreshToken.RevokedAtUtc);
        Assert.Null(refreshToken.ReplacedByTokenId);
        Assert.True(refreshToken.IsActive(DateTime.UtcNow));
    }

    [Fact]
    public void ConstructorWithEmptyUserIdThrowsArgumentException()
    {
        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new RefreshToken(
                    Guid.Empty,
                    ValidTokenHash,
                    DateTime.UtcNow.AddDays(7)));

        Assert.Equal("userId", exception.ParamName);
    }

    [Fact]
    public void ConstructorWithBlankHashThrowsArgumentException()
    {
        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new RefreshToken(
                    Guid.NewGuid(),
                    " ",
                    DateTime.UtcNow.AddDays(7)));

        Assert.Equal("tokenHash", exception.ParamName);
    }

    [Fact]
    public void ConstructorWithNonUtcExpirationThrowsArgumentException()
    {
        DateTime localExpiration = DateTime.SpecifyKind(
            DateTime.UtcNow.AddDays(7),
            DateTimeKind.Local);

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new RefreshToken(
                    Guid.NewGuid(),
                    ValidTokenHash,
                    localExpiration));

        Assert.Equal("expiresAtUtc", exception.ParamName);
    }

    [Fact]
    public void ConstructorWithExpiredDateThrowsArgumentException()
    {
        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => new RefreshToken(
                    Guid.NewGuid(),
                    ValidTokenHash,
                    DateTime.UtcNow.AddMinutes(-1)));

        Assert.Equal("expiresAtUtc", exception.ParamName);
    }

    [Fact]
    public void IsActiveAfterExpirationReturnsFalse()
    {
        DateTime expiresAtUtc = DateTime.UtcNow.AddMinutes(1);

        var refreshToken = new RefreshToken(
            Guid.NewGuid(),
            ValidTokenHash,
            expiresAtUtc);

        bool isActive =
            refreshToken.IsActive(
                expiresAtUtc.AddSeconds(1));

        Assert.False(isActive);
    }

    [Fact]
    public void RevokeWithValidDataDeactivatesToken()
    {
        var refreshToken = CreateRefreshToken();
        Guid replacementId = Guid.NewGuid();
        DateTime revokedAtUtc = DateTime.UtcNow;

        refreshToken.Revoke(
            revokedAtUtc,
            replacementId);

        Assert.Equal(
            revokedAtUtc,
            refreshToken.RevokedAtUtc);

        Assert.Equal(
            replacementId,
            refreshToken.ReplacedByTokenId);

        Assert.False(
            refreshToken.IsActive(
                revokedAtUtc.AddSeconds(1)));
    }

    [Fact]
    public void RevokeAlreadyRevokedTokenThrowsInvalidOperationException()
    {
        var refreshToken = CreateRefreshToken();

        refreshToken.Revoke(DateTime.UtcNow);

        Assert.Throws<InvalidOperationException>(
            () => refreshToken.Revoke(
                DateTime.UtcNow.AddSeconds(1)));
    }

    [Fact]
    public void RevokeWithOwnIdThrowsArgumentException()
    {
        var refreshToken = CreateRefreshToken();

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () => refreshToken.Revoke(
                    DateTime.UtcNow,
                    refreshToken.Id));

        Assert.Equal(
            "replacedByTokenId",
            exception.ParamName);
    }

    private static RefreshToken CreateRefreshToken()
    {
        return new RefreshToken(
            Guid.NewGuid(),
            ValidTokenHash,
            DateTime.UtcNow.AddDays(7));
    }
}
