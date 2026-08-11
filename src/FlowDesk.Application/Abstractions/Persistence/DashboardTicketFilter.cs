namespace FlowDesk.Application.Abstractions.Persistence;

public sealed record DashboardTicketFilter(
    Guid? CompanyId,
    Guid? RequesterId);
