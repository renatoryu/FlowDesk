namespace FlowDesk.Application.Comments.Create;

public sealed record CreateCommentResult(
    Guid Id,
    Guid TicketId,
    Guid AuthorId,
    string Content,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
