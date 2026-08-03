using FlowDesk.Application.Abstractions.Security;
using Microsoft.AspNetCore.Identity;

namespace FlowDesk.Infrastructure.Security;

public sealed class AspNetCorePasswordHasher : IPasswordHasher
{
    private static readonly object UserContext = new();

    private readonly PasswordHasher<object> _passwordHasher = new();

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return _passwordHasher.HashPassword(
            UserContext,
            password);
    }

    public bool Verify(string passwordHash, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        PasswordVerificationResult result =
            _passwordHasher.VerifyHashedPassword(
                UserContext,
                passwordHash,
                password);

        return result is
            PasswordVerificationResult.Success or
            PasswordVerificationResult.SuccessRehashNeeded;
    }
}
