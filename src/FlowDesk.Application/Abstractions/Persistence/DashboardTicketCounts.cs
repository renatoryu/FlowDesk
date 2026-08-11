namespace FlowDesk.Application.Abstractions.Persistence;

public sealed record DashboardTicketCounts(
    int OpenTickets,
    int InProgressTickets,
    int CompletedTickets);
