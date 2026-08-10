using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Abstractions.Persistence;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> ListActiveAsync(
        CancellationToken cancellationToken = default);
}
