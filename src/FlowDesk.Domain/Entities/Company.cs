using System.Net.Mail;
using FlowDesk.Domain.Common;

namespace FlowDesk.Domain.Entities;

public sealed class Company : BaseEntity
{
    public const int MaxNameLength = 150;
    public const int TaxIdLength = 14;
    public const int MaxContactEmailLength = 254;
    public const int MaxTaxIdInputLength = 18;

    private Company()
    {
    }

    public Company(
        string name,
        string taxId,
        string contactEmail)
    {
        Name = NormalizeName(name);
        TaxId = NormalizeTaxId(taxId);
        ContactEmail = NormalizeContactEmail(contactEmail);
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;

    public string TaxId { get; private set; } = string.Empty;

    public string ContactEmail { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public static bool IsValidTaxId(string? taxId)
    {
        if (string.IsNullOrWhiteSpace(taxId))
        {
            return false;
        }

        bool containsInvalidCharacter = taxId.Any(
            character =>
                !IsAsciiDigit(character) &&
                character is not '.' and not '/' and not '-' &&
                !char.IsWhiteSpace(character));

        if (containsInvalidCharacter)
        {
            return false;
        }

        string normalized =
            RemoveTaxIdFormatting(taxId);

        return normalized.Length == TaxIdLength &&
            !normalized.All(
                digit => digit == normalized[0]) &&
            HasValidCheckDigits(normalized);
    }

    public static bool IsValidContactEmail(
        string? contactEmail)
    {
        if (string.IsNullOrWhiteSpace(contactEmail))
        {
            return false;
        }

        string normalized =
            contactEmail.Trim();

        return normalized.Length <= MaxContactEmailLength &&
            MailAddress.TryCreate(
                normalized,
                out MailAddress? parsedEmail) &&
            string.Equals(
                parsedEmail.Address,
                normalized,
                StringComparison.OrdinalIgnoreCase);
    }

    public void UpdateDetails(
        string name,
        string contactEmail)
    {
        string normalizedName =
            NormalizeName(name);

        string normalizedContactEmail =
            NormalizeContactEmail(contactEmail);

        Name = normalizedName;
        ContactEmail = normalizedContactEmail;
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

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        string normalized = name.Trim();

        if (normalized.Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"Company name cannot exceed {MaxNameLength} characters.",
                nameof(name));
        }

        return normalized;
    }

    private static string NormalizeTaxId(string taxId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taxId);

        if (!IsValidTaxId(taxId))
        {
            throw new ArgumentException(
                "Tax id must be a valid CNPJ.",
                nameof(taxId));
        }

        return RemoveTaxIdFormatting(taxId);
    }

    private static string NormalizeContactEmail(
        string contactEmail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            contactEmail);

        string normalized =
            contactEmail.Trim().ToLowerInvariant();

        if (normalized.Length > MaxContactEmailLength)
        {
            throw new ArgumentException(
                $"Contact email cannot exceed {MaxContactEmailLength} characters.",
                nameof(contactEmail));
        }

        if (!IsValidContactEmail(normalized))
        {
            throw new ArgumentException(
                "Contact email is invalid.",
                nameof(contactEmail));
        }

        return normalized;
    }

    private static string RemoveTaxIdFormatting(
        string taxId)
    {
        return new string(
            taxId
                .Where(IsAsciiDigit)
                .ToArray());
    }

    private static bool IsAsciiDigit(
        char character)
    {
        return character is >= '0' and <= '9';
    }

    private static bool HasValidCheckDigits(
        string taxId)
    {
        int firstCheckDigit =
            CalculateCheckDigit(
                taxId.AsSpan(0, 12),
                initialWeight: 5);

        int secondCheckDigit =
            CalculateCheckDigit(
                taxId.AsSpan(0, 13),
                initialWeight: 6);

        return taxId[12] - '0' == firstCheckDigit &&
            taxId[13] - '0' == secondCheckDigit;
    }

    private static int CalculateCheckDigit(
        ReadOnlySpan<char> digits,
        int initialWeight)
    {
        int sum = 0;

        for (int index = 0;
             index < digits.Length;
             index++)
        {
            int weight = initialWeight - index;

            if (weight < 2)
            {
                weight += 8;
            }

            sum += (digits[index] - '0') * weight;
        }

        int remainder = sum % 11;

        return remainder < 2
            ? 0
            : 11 - remainder;
    }
}
