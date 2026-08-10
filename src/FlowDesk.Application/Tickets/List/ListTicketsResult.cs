namespace FlowDesk.Application.Tickets.List;

public sealed record ListTicketsResult(
    IReadOnlyList<TicketListItem> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
