using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Persistence.Repositories;

public sealed class CommentRepository : ICommentRepository
{
    private readonly FlowDeskDbContext _dbContext;

    public CommentRepository(
        FlowDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Comment comment,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Comments.AddAsync(
            comment,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Comment>> ListByTicketIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Comments
            .AsNoTracking()
            .Where(comment => comment.TicketId == ticketId)
            .OrderBy(comment => comment.CreatedAtUtc)
            .ThenBy(comment => comment.Id)
            .ToArrayAsync(cancellationToken);
    }
}
