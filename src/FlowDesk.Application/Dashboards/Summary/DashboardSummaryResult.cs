namespace FlowDesk.Application.Dashboards.Summary;

public sealed record DashboardSummaryResult(
    int OpenTickets,
    int InProgressTickets,
    int CompletedTickets);
