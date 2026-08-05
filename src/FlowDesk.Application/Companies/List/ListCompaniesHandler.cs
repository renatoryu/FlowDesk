using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Companies.List;

public sealed class ListCompaniesHandler
{
    private readonly ICompanyRepository _companyRepository;

    public ListCompaniesHandler(
        ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<IReadOnlyList<ListCompanyResult>> HandleAsync(
        ListCompaniesQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Company> companies =
            await _companyRepository.ListAsync(
                query.IncludeInactive,
                cancellationToken);

        return companies
            .Select(company => new ListCompanyResult(
                company.Id,
                company.Name,
                company.TaxId,
                company.ContactEmail,
                company.IsActive,
                company.CreatedAtUtc,
                company.UpdatedAtUtc))
            .ToArray();
    }
}
