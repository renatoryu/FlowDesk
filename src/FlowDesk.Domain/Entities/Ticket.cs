using FlowDesk.Domain.Common;
using FlowDesk.Domain.Enums;

namespace FlowDesk.Domain.Entities;

public sealed class Ticket : BaseEntity
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 4000;

    private Ticket()
    {
    }

    public Ticket(
        Guid companyId,
        Guid categoryId,
        Guid requesterId,
        string title,
        string description,
        TicketPriority priority)
    {
        CompanyId = ValidateRequiredId(
            companyId,
            nameof(companyId));

        CategoryId = ValidateRequiredId(
            categoryId,
            nameof(categoryId));

        RequesterId = ValidateRequiredId(
            requesterId,
            nameof(requesterId));

        Title = NormalizeTitle(title);
        Description = NormalizeDescription(description);
        Priority = ValidatePriority(priority);
        Status = TicketStatus.Open;
        StatusChangedAtUtc = CreatedAtUtc;
        IsDeleted = false;
    }

    public Guid CompanyId { get; private set; }

    public Guid CategoryId { get; private set; }

    public Guid RequesterId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } =
        string.Empty;

    public TicketPriority Priority { get; private set; }

    public TicketStatus Status { get; private set; }

    public DateTime StatusChangedAtUtc { get; private set; }

    public DateTime? ResolvedAtUtc { get; private set; }

    public DateTime? ClosedAtUtc { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAtUtc { get; private set; }

    public Guid? DeletedByUserId { get; private set; }

    public void UpdateDetails(
        string title,
        string description,
        Guid categoryId,
        TicketPriority priority)
    {
        EnsureNotDeleted();

        if (Status == TicketStatus.Closed)
        {
            throw new DomainRuleException(
                "Closed tickets cannot be edited.");
        }

        string normalizedTitle =
            NormalizeTitle(title);

        string normalizedDescription =
            NormalizeDescription(description);

        Guid validatedCategoryId =
            ValidateRequiredId(
                categoryId,
                nameof(categoryId));

        TicketPriority validatedPriority =
            ValidatePriority(priority);

        if (string.Equals(
                Title,
                normalizedTitle,
                StringComparison.Ordinal) &&
            string.Equals(
                Description,
                normalizedDescription,
                StringComparison.Ordinal) &&
            CategoryId == validatedCategoryId &&
            Priority == validatedPriority)
        {
            return;
        }

        Title = normalizedTitle;
        Description = normalizedDescription;
        CategoryId = validatedCategoryId;
        Priority = validatedPriority;
        MarkAsUpdated();
    }

    public void ChangeStatus(TicketStatus status)
    {
        EnsureNotDeleted();

        TicketStatus validatedStatus =
            ValidateStatus(status);

        if (Status == validatedStatus)
        {
            return;
        }

        if (!IsTransitionAllowed(
                Status,
                validatedStatus))
        {
            throw new DomainRuleException(
                $"Ticket status cannot change from {Status} to {validatedStatus}.");
        }

        DateTime changedAtUtc = DateTime.UtcNow;

        Status = validatedStatus;
        StatusChangedAtUtc = changedAtUtc;

        if (validatedStatus == TicketStatus.Resolved)
        {
            ResolvedAtUtc = changedAtUtc;
            ClosedAtUtc = null;
        }
        else if (validatedStatus == TicketStatus.Closed)
        {
            ClosedAtUtc = changedAtUtc;
        }
        else
        {
            ResolvedAtUtc = null;
            ClosedAtUtc = null;
        }

        MarkAsUpdated();
    }

    public void Delete(Guid deletedByUserId)
    {
        Guid validatedDeletedByUserId =
            ValidateRequiredId(
                deletedByUserId,
                nameof(deletedByUserId));

        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        DeletedByUserId = validatedDeletedByUserId;
        MarkAsUpdated();
    }

    public void EnsureCanReceiveComments()
    {
        EnsureNotDeleted();

        if (Status == TicketStatus.Closed)
        {
            throw new DomainRuleException(
                "Closed tickets cannot receive comments.");
        }
    }

    public void EnsureCanReceiveAttachments()
    {
        EnsureNotDeleted();

        if (Status == TicketStatus.Closed)
        {
            throw new DomainRuleException(
                "Closed tickets cannot receive attachments.");
        }
    }

    private static Guid ValidateRequiredId(
        Guid id,
        string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Identifier cannot be empty.",
                parameterName);
        }

        return id;
    }

    private static string NormalizeTitle(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        string normalized = title.Trim();

        if (normalized.Length > MaxTitleLength)
        {
            throw new ArgumentException(
                $"Ticket title cannot exceed {MaxTitleLength} characters.",
                nameof(title));
        }

        return normalized;
    }

    private static string NormalizeDescription(
        string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            description);

        string normalized = description.Trim();

        if (normalized.Length > MaxDescriptionLength)
        {
            throw new ArgumentException(
                $"Ticket description cannot exceed {MaxDescriptionLength} characters.",
                nameof(description));
        }

        return normalized;
    }

    private static TicketPriority ValidatePriority(
        TicketPriority priority)
    {
        if (!Enum.IsDefined(priority))
        {
            throw new ArgumentOutOfRangeException(
                nameof(priority));
        }

        return priority;
    }

    private static TicketStatus ValidateStatus(
        TicketStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status));
        }

        return status;
    }

    private static bool IsTransitionAllowed(
        TicketStatus currentStatus,
        TicketStatus newStatus)
    {
        return currentStatus switch
        {
            TicketStatus.Open =>
                newStatus == TicketStatus.InProgress,

            TicketStatus.InProgress =>
                newStatus is
                    TicketStatus.Open or
                    TicketStatus.Resolved,

            TicketStatus.Resolved =>
                newStatus is
                    TicketStatus.InProgress or
                    TicketStatus.Closed,

            TicketStatus.Closed => false,

            _ => false
        };
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new DomainRuleException(
                "Deleted tickets cannot be changed.");
        }
    }
}
