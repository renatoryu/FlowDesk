using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace FlowDesk.Infrastructure.Authentication;

public sealed class JwtTokenGenerator : IAccessTokenGenerator
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _signingCredentials;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public JwtTokenGenerator(JwtOptions options)
    {
        _options = options;

        byte[] signingKeyBytes =
            Convert.FromBase64String(options.SigningKey);

        var securityKey =
            new SymmetricSecurityKey(signingKeyBytes);

        _signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);
    }

    public AccessTokenResult Generate(User user)
    {
        DateTime issuedAtUtc = DateTime.UtcNow;

        DateTime expiresAtUtc =
            issuedAtUtc.AddMinutes(
                _options.AccessTokenExpirationMinutes);

        Claim[] claims =
        [
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString("D")),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new(
                JwtRegisteredClaimNames.Name,
                user.FullName),

            new(
                "role",
                user.Role.ToString()),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString("D")),

            new(
                JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(issuedAtUtc)
                    .ToUnixTimeSeconds()
                    .ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64)
        ];

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAtUtc,
            expires: expiresAtUtc,
            signingCredentials: _signingCredentials);

        string serializedToken =
            _tokenHandler.WriteToken(token);

        return new AccessTokenResult(
            serializedToken,
            expiresAtUtc);
    }
}
