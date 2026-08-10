namespace FlowDesk.Application.Comments.List;

public sealed record CommentHistoryItem(
    Guid Id,
    Guid TicketId,
    Guid AuthorId,
    string Content,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
