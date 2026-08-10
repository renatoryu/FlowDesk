using FlowDesk.Domain.Enums;

namespace FlowDesk.Application.Tickets.Create;

public sealed record CreateTicketResult(
    Guid Id,
    Guid CompanyId,
    Guid CategoryId,
    Guid RequesterId,
    string Title,
    string Description,
    TicketPriority Priority,
    TicketStatus Status,
    DateTime CreatedAtUtc,
    DateTime StatusChangedAtUtc);
