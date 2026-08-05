using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Companies.GetById;

public sealed class GetCompanyByIdHandler
{
    private readonly ICompanyRepository _companyRepository;

    public GetCompanyByIdHandler(
        ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<GetCompanyByIdResult> HandleAsync(
        GetCompanyByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        Company company =
            await _companyRepository.GetByIdAsync(
                query.Id,
                cancellationToken)
            ?? throw new NotFoundException(
                "Company was not found.");

        return new GetCompanyByIdResult(
            company.Id,
            company.Name,
            company.TaxId,
            company.ContactEmail,
            company.IsActive,
            company.CreatedAtUtc,
            company.UpdatedAtUtc);
    }
}
