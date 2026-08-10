using FlowDesk.Domain.Enums;

namespace FlowDesk.Application.Tickets.Update;

public sealed record UpdateTicketResult(
    Guid Id,
    Guid CompanyId,
    Guid CategoryId,
    Guid RequesterId,
    string Title,
    string Description,
    TicketPriority Priority,
    TicketStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime StatusChangedAtUtc,
    DateTime? ResolvedAtUtc,
    DateTime? ClosedAtUtc);
