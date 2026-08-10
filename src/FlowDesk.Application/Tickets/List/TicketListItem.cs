using FlowDesk.Domain.Enums;

namespace FlowDesk.Application.Tickets.List;

public sealed record TicketListItem(
    Guid Id,
    Guid CompanyId,
    Guid CategoryId,
    Guid RequesterId,
    string Title,
    TicketPriority Priority,
    TicketStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime StatusChangedAtUtc);
