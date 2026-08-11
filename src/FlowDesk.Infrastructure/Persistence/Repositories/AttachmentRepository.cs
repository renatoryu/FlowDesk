using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Persistence.Repositories;

public sealed class AttachmentRepository
    : IAttachmentRepository
{
    private readonly FlowDeskDbContext _dbContext;

    public AttachmentRepository(
        FlowDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Attachment?> GetByIdAsync(
        Guid ticketId,
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Attachments
            .AsNoTracking()
            .SingleOrDefaultAsync(
                attachment =>
                    attachment.Id == attachmentId &&
                    attachment.TicketId == ticketId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Attachment>> ListByTicketIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Attachments
            .AsNoTracking()
            .Where(attachment =>
                attachment.TicketId == ticketId)
            .OrderBy(attachment =>
                attachment.CreatedAtUtc)
            .ThenBy(attachment =>
                attachment.Id)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddAsync(
        Attachment attachment,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Attachments.AddAsync(
            attachment,
            cancellationToken);
    }
}
