using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<User?> GetForUpdateAsync(
    Guid userId,
    CancellationToken cancellationToken = default);

    Task AddAsync(
        User user,
        CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
