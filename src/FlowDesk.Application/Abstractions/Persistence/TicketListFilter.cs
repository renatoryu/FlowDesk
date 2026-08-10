using FlowDesk.Domain.Enums;

namespace FlowDesk.Application.Abstractions.Persistence;

public sealed record TicketListFilter(
    Guid? CompanyId,
    Guid? RequesterId,
    Guid? CategoryId,
    TicketPriority? Priority,
    TicketStatus? Status,
    int Page,
    int PageSize);
