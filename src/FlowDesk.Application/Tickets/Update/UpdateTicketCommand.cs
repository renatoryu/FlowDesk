using FlowDesk.Domain.Enums;

namespace FlowDesk.Application.Tickets.Update;

public sealed record UpdateTicketCommand(
    Guid Id,
    Guid CategoryId,
    string Title,
    string Description,
    TicketPriority Priority);
