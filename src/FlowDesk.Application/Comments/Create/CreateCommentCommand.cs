namespace FlowDesk.Application.Comments.Create;

public sealed record CreateCommentCommand(
    Guid TicketId,
    string Content);
