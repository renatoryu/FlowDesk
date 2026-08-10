using FlowDesk.Domain.Enums;

namespace FlowDesk.Application.Tickets.ChangeStatus;

public sealed record ChangeTicketStatusResult(
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
