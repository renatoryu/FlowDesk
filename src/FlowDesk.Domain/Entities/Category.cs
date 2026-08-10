using FlowDesk.Domain.Common;

namespace FlowDesk.Domain.Entities;

public sealed class Category : BaseEntity
{
    public const int MaxNameLength = 100;
    public const int MaxDescriptionLength = 500;

    private Category()
    {
    }

    public Category(
        string name,
        string? description = null)
    {
        Name = NormalizeName(name);
        NormalizedName = NormalizeNameKey(Name);
        Description = NormalizeDescription(description);
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } =
        string.Empty;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public void UpdateDetails(
        string name,
        string? description)
    {
        string normalizedName = NormalizeName(name);

        string normalizedNameKey =
            NormalizeNameKey(normalizedName);

        string? normalizedDescription =
            NormalizeDescription(description);

        if (string.Equals(
                Name,
                normalizedName,
                StringComparison.Ordinal) &&
            string.Equals(
                Description,
                normalizedDescription,
                StringComparison.Ordinal))
        {
            return;
        }

        Name = normalizedName;
        NormalizedName = normalizedNameKey;
        Description = normalizedDescription;
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
                $"Category name cannot exceed {MaxNameLength} characters.",
                nameof(name));
        }

        return normalized;
    }

    private static string NormalizeNameKey(string name)
    {
        return name.ToUpperInvariant();
    }

    private static string? NormalizeDescription(
        string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        string normalized = description.Trim();

        if (normalized.Length > MaxDescriptionLength)
        {
            throw new ArgumentException(
                $"Category description cannot exceed {MaxDescriptionLength} characters.",
                nameof(description));
        }

        return normalized;
    }
}
