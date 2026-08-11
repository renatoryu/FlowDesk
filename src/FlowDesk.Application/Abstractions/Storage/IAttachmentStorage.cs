namespace FlowDesk.Application.Abstractions.Storage;

public interface IAttachmentStorage
{
    Task<string> SaveAsync(
        Guid ticketId,
        Stream content,
        string fileExtension,
        CancellationToken cancellationToken = default);

    Task<Stream?> OpenReadAsync(
        Guid ticketId,
        string storedFileName,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid ticketId,
        string storedFileName,
        CancellationToken cancellationToken = default);
}
