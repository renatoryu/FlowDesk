using FlowDesk.Domain.Enums;

namespace FlowDesk.Application.Tickets.List;

public sealed record ListTicketsQuery(
    int Page = 1,
    int PageSize = 20,
    TicketStatus? Status = null,
    TicketPriority? Priority = null,
    Guid? CategoryId = null);
