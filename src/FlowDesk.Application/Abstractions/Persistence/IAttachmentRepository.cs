using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Abstractions.Persistence;

public interface IAttachmentRepository
{
    Task<Attachment?> GetByIdAsync(
        Guid ticketId,
        Guid attachmentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Attachment>> ListByTicketIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Attachment attachment,
        CancellationToken cancellationToken = default);
}
