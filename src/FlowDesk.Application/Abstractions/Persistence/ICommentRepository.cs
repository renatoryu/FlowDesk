using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Abstractions.Persistence;

public interface ICommentRepository
{
    Task AddAsync(
        Comment comment,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Comment>> ListByTicketIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);
}
