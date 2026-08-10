namespace FlowDesk.Application.Comments.List;

public sealed record ListTicketCommentsResult(
    IReadOnlyList<CommentHistoryItem> Items);
