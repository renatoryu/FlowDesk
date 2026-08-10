using FlowDesk.Domain.Enums;

namespace FlowDesk.Api.Contracts.Tickets;

public sealed record UpdateTicketRequest(
    Guid CategoryId,
    string Title,
    string Description,
    TicketPriority Priority);
