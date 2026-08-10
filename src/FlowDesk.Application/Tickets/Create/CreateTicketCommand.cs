using FlowDesk.Domain.Enums;

namespace FlowDesk.Application.Tickets.Create;

public sealed record CreateTicketCommand(
    Guid CategoryId,
    string Title,
    string Description,
    TicketPriority Priority);
