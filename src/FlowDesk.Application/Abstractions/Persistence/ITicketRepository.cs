using FlowDesk.Application.Common.Models;
using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Abstractions.Persistence;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);

    Task<Ticket?> GetForUpdateAsync(
        Guid ticketId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);
    Task<PagedResult<Ticket>> ListAsync(
        TicketListFilter filter,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Ticket ticket,
        CancellationToken cancellationToken = default);
}
