using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Persistence.Repositories;

public sealed class CompanyRepository : ICompanyRepository
{
    private readonly FlowDeskDbContext _dbContext;

    public CompanyRepository(
        FlowDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsByTaxIdAsync(
        string taxId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Companies.AnyAsync(
            company => company.TaxId == taxId,
            cancellationToken);
    }

    public Task<Company?> GetByIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Companies
            .SingleOrDefaultAsync(
                company => company.Id == companyId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Company>> ListAsync(
        bool includeInactive,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Company> query =
            _dbContext.Companies.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(
                company => company.IsActive);
        }

        return await query
            .OrderBy(company => company.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Company company,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Companies.AddAsync(
            company,
            cancellationToken);
    }
}
