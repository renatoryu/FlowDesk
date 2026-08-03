using System.Security.Cryptography;
using System.Text;
using FlowDesk.Application.Abstractions.Security;
using Microsoft.IdentityModel.Tokens;

namespace FlowDesk.Infrastructure.Authentication;

public sealed class RefreshTokenGenerator
    : IRefreshTokenGenerator
{
    private const int TokenSizeInBytes = 64;

    private readonly JwtOptions _options;

    public RefreshTokenGenerator(JwtOptions options)
    {
        _options = options;
    }

    public GeneratedRefreshToken Generate()
    {
        byte[] randomBytes =
            RandomNumberGenerator.GetBytes(
                TokenSizeInBytes);

        string token =
            Base64UrlEncoder.Encode(randomBytes);

        string tokenHash =
            ComputeHash(token);

        DateTime expiresAtUtc =
            DateTime.UtcNow.AddDays(
                _options.RefreshTokenExpirationDays);

        return new GeneratedRefreshToken(
            token,
            tokenHash,
            expiresAtUtc);
    }

    public string ComputeHash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        byte[] tokenBytes =
            Encoding.UTF8.GetBytes(token);

        byte[] hashBytes =
            SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }
}
