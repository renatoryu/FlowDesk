namespace FlowDesk.Application.Abstractions.Persistence;

public interface IDashboardRepository
{
    Task<DashboardTicketCounts> GetTicketCountsAsync(
        DashboardTicketFilter filter,
        CancellationToken cancellationToken = default);
}
