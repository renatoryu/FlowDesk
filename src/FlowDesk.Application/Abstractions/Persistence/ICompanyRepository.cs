using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Abstractions.Persistence;

public interface ICompanyRepository
{
    Task<bool> ExistsByTaxIdAsync(
        string taxId,
        CancellationToken cancellationToken = default);

    Task<Company?> GetByIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Company>> ListAsync(
        bool includeInactive,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Company company,
        CancellationToken cancellationToken = default);
}
