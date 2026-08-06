using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Persistence.Repositories;

public sealed class TicketRepository : ITicketRepository
{
    private readonly FlowDeskDbContext _dbContext;

    public TicketRepository(
        FlowDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Ticket?> GetByIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Tickets
            .AsNoTracking()
            .Where(ticket => !ticket.IsDeleted)
            .SingleOrDefaultAsync(
                ticket => ticket.Id == ticketId,
                cancellationToken);
    }

    public Task<Ticket?> GetForUpdateAsync(
        Guid ticketId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Ticket> query =
            _dbContext.Tickets;

        if (!includeDeleted)
        {
            query = query.Where(
                ticket => !ticket.IsDeleted);
        }

        return query.SingleOrDefaultAsync(
            ticket => ticket.Id == ticketId,
            cancellationToken);
    }

    public async Task AddAsync(
        Ticket ticket,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Tickets.AddAsync(
            ticket,
            cancellationToken);
    }
}
