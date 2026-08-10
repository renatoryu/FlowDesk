using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Common.Models;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
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

    public async Task<PagedResult<Ticket>> ListAsync(
    TicketListFilter filter,
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

        if (filter.CategoryId is Guid categoryId)
        {
            query = query.Where(
                ticket => ticket.CategoryId == categoryId);
        }

        if (filter.Priority is TicketPriority priority)
        {
            query = query.Where(
                ticket => ticket.Priority == priority);
        }

        if (filter.Status is TicketStatus status)
        {
            query = query.Where(
                ticket => ticket.Status == status);
        }

        int totalCount =
            await query.CountAsync(cancellationToken);

        Ticket[] tickets =
            await query
                .OrderByDescending(
                    ticket => ticket.CreatedAtUtc)
                .ThenByDescending(
                    ticket => ticket.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToArrayAsync(cancellationToken);

        return new PagedResult<Ticket>(
            tickets,
            filter.Page,
            filter.PageSize,
            totalCount);
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
