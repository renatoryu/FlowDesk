using System.Net.Mail;
using FlowDesk.Domain.Common;

namespace FlowDesk.Domain.Entities;

public sealed class Company : BaseEntity
{
    public const int MaxNameLength = 150;
    public const int TaxIdLength = 14;
    public const int MaxContactEmailLength = 254;

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

        bool containsInvalidCharacter = taxId.Any(
            character =>
                !char.IsDigit(character) &&
                character is not '.' and not '/' and not '-' &&
                !char.IsWhiteSpace(character));

        if (containsInvalidCharacter)
        {
            throw new ArgumentException(
                "Tax id contains invalid characters.",
                nameof(taxId));
        }

        string normalized = new(
            taxId
                .Where(char.IsDigit)
                .ToArray());

        if (normalized.Length != TaxIdLength ||
            normalized.All(digit => digit == normalized[0]) ||
            !HasValidCheckDigits(normalized))
        {
            throw new ArgumentException(
                "Tax id must be a valid CNPJ.",
                nameof(taxId));
        }

        return normalized;
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

        if (!MailAddress.TryCreate(
                normalized,
                out MailAddress? parsedEmail) ||
            !string.Equals(
                parsedEmail.Address,
                normalized,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Contact email is invalid.",
                nameof(contactEmail));
        }

        return normalized;
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
