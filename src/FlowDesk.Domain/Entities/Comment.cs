using FlowDesk.Domain.Common;

namespace FlowDesk.Domain.Entities;

public sealed class Comment : BaseEntity
{
    public const int MaxContentLength = 2000;

    private Comment()
    {
    }

    public Comment(
        Guid ticketId,
        Guid authorId,
        string content)
    {
        TicketId = ValidateRequiredId(
            ticketId,
            nameof(ticketId));

        AuthorId = ValidateRequiredId(
            authorId,
            nameof(authorId));

        Content = NormalizeContent(content);
    }

    public Guid TicketId { get; private set; }

    public Guid AuthorId { get; private set; }

    public string Content { get; private set; } = string.Empty;

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

    private static string NormalizeContent(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        string normalized = content.Trim();

        if (normalized.Length > MaxContentLength)
        {
            throw new ArgumentException(
                $"Comment content cannot exceed {MaxContentLength} characters.",
                nameof(content));
        }

        return normalized;
    }
}
