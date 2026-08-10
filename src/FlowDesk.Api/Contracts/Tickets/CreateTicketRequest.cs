using FlowDesk.Domain.Enums;

namespace FlowDesk.Api.Contracts.Tickets;

public sealed record CreateTicketRequest(
    Guid CategoryId,
    string Title,
    string Description,
    TicketPriority Priority);
