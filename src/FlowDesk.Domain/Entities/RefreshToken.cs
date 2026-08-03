using FlowDesk.Domain.Common;

namespace FlowDesk.Domain.Entities;

public sealed class RefreshToken : BaseEntity
{
    public const int MaxTokenHashLength = 128;

    private RefreshToken()
    {
    }

    public RefreshToken(
        Guid userId,
        string tokenHash,
        DateTime expiresAtUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User id cannot be empty.",
                nameof(userId));
        }

        UserId = userId;
        TokenHash = ValidateTokenHash(tokenHash);
        ExpiresAtUtc = ValidateExpiration(expiresAtUtc);
    }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsActive(DateTime utcNow)
    {
        EnsureUtc(utcNow, nameof(utcNow));

        return RevokedAtUtc is null &&
            utcNow < ExpiresAtUtc;
    }

    public void Revoke(
        DateTime revokedAtUtc,
        Guid? replacedByTokenId = null)
    {
        EnsureUtc(revokedAtUtc, nameof(revokedAtUtc));

        if (RevokedAtUtc is not null)
        {
            throw new InvalidOperationException(
                "Refresh token is already revoked.");
        }

        if (revokedAtUtc < CreatedAtUtc)
        {
            throw new ArgumentException(
                "Revocation cannot precede token creation.",
                nameof(revokedAtUtc));
        }

        if (replacedByTokenId == Guid.Empty ||
            replacedByTokenId == Id)
        {
            throw new ArgumentException(
                "Replacement token id is invalid.",
                nameof(replacedByTokenId));
        }

        RevokedAtUtc = revokedAtUtc;
        ReplacedByTokenId = replacedByTokenId;
        MarkAsUpdated();
    }

    private static string ValidateTokenHash(string tokenHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        string normalized = tokenHash.Trim();

        if (normalized.Length > MaxTokenHashLength)
        {
            throw new ArgumentException(
                $"Token hash cannot exceed {MaxTokenHashLength} characters.",
                nameof(tokenHash));
        }

        return normalized;
    }

    private DateTime ValidateExpiration(DateTime expiresAtUtc)
    {
        EnsureUtc(expiresAtUtc, nameof(expiresAtUtc));

        if (expiresAtUtc <= CreatedAtUtc)
        {
            throw new ArgumentException(
                "Refresh token expiration must be in the future.",
                nameof(expiresAtUtc));
        }

        return expiresAtUtc;
    }

    private static void EnsureUtc(
        DateTime value,
        string parameterName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "Date and time value must use UTC.",
                parameterName);
        }
    }
}
