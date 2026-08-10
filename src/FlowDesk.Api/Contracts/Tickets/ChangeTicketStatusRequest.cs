using FlowDesk.Domain.Enums;

namespace FlowDesk.Api.Contracts.Tickets;

public sealed record ChangeTicketStatusRequest(
    TicketStatus Status);
