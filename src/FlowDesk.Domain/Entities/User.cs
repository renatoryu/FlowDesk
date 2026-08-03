using System.Net.Mail;
using FlowDesk.Domain.Common;
using FlowDesk.Domain.Enums;

namespace FlowDesk.Domain.Entities;

public sealed class User : BaseEntity
{
    public const int MaxFullNameLength = 150;
    public const int MaxEmailLength = 254;
    public const int MaxPasswordHashLength = 1024;

    private User()
    {
    }

    public User(
        string fullName,
        string email,
        string passwordHash,
        UserRole role = UserRole.Customer)
    {
        FullName = NormalizeFullName(fullName);
        Email = NormalizeEmail(email);
        PasswordHash = ValidatePasswordHash(passwordHash);
        Role = ValidateRole(role);
        IsActive = true;
    }

    public string FullName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public void UpdateProfile(string fullName, string email)
    {
        FullName = NormalizeFullName(fullName);
        Email = NormalizeEmail(email);
        MarkAsUpdated();
    }

    public void ChangePasswordHash(string passwordHash)
    {
        PasswordHash = ValidatePasswordHash(passwordHash);
        MarkAsUpdated();
    }

    public void ChangeRole(UserRole role)
    {
        Role = ValidateRole(role);
        MarkAsUpdated();
    }

    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        MarkAsUpdated();
    }

    private static string NormalizeFullName(string fullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

        string normalized = fullName.Trim();

        if (normalized.Length > MaxFullNameLength)
        {
            throw new ArgumentException(
                $"Full name cannot exceed {MaxFullNameLength} characters.",
                nameof(fullName));
        }

        return normalized;
    }

    private static string NormalizeEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        string normalized = email.Trim().ToLowerInvariant();

        if (normalized.Length > MaxEmailLength)
        {
            throw new ArgumentException(
                $"Email cannot exceed {MaxEmailLength} characters.",
                nameof(email));
        }

        if (!MailAddress.TryCreate(normalized, out MailAddress? parsedEmail) ||
            !string.Equals(
                parsedEmail.Address,
                normalized,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Email is invalid.", nameof(email));
        }

        return normalized;
    }

    private static string ValidatePasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        if (passwordHash.Length > MaxPasswordHashLength)
        {
            throw new ArgumentException(
                $"Password hash cannot exceed {MaxPasswordHashLength} characters.",
                nameof(passwordHash));
        }

        return passwordHash;
    }

    private static UserRole ValidateRole(UserRole role)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }

        return role;
    }
}
