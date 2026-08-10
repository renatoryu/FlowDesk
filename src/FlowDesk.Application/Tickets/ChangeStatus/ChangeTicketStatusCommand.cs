using FlowDesk.Domain.Enums;

namespace FlowDesk.Application.Tickets.ChangeStatus;

public sealed record ChangeTicketStatusCommand(
    Guid Id,
    TicketStatus Status);
