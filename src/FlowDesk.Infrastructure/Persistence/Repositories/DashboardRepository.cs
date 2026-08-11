using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Persistence.Repositories;

public sealed class DashboardRepository : IDashboardRepository
{
    private readonly FlowDeskDbContext _dbContext;

    public DashboardRepository(
        FlowDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardTicketCounts> GetTicketCountsAsync(
        DashboardTicketFilter filter,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Ticket> query =
            _dbContext.Tickets
                .AsNoTracking()
                .Where(ticket => !ticket.IsDeleted);

        if (filter.CompanyId is Guid companyId)
        {
            query = query.Where(
                ticket => ticket.CompanyId == companyId);
        }

        if (filter.RequesterId is Guid requesterId)
        {
            query = query.Where(
                ticket => ticket.RequesterId == requesterId);
        }

        DashboardTicketCounts? counts =
            await query
                .GroupBy(_ => 1)
                .Select(group => new DashboardTicketCounts(
                    group.Count(
                        ticket => ticket.Status == TicketStatus.Open),
                    group.Count(
                        ticket => ticket.Status ==
                            TicketStatus.InProgress),
                    group.Count(
                        ticket => ticket.Status ==
                            TicketStatus.Resolved ||
                            ticket.Status ==
                            TicketStatus.Closed)))
                .SingleOrDefaultAsync(cancellationToken);

        return counts ?? new DashboardTicketCounts(
            0,
            0,
            0);
    }
}
