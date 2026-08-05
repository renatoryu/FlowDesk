using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Companies.GetById;
using FlowDesk.Domain.Entities;

namespace FlowDesk.UnitTests.Application.Companies.GetById;

public sealed class GetCompanyByIdHandlerTests
{
    [Fact]
    public async Task HandleAsyncWithExistingCompanyReturnsCompany()
    {
        Company company = CreateCompany();

        var repository =
            new CompanyRepositoryStub(company);

        var handler =
            new GetCompanyByIdHandler(repository);

        var query =
            new GetCompanyByIdQuery(company.Id);

        GetCompanyByIdResult result =
            await handler.HandleAsync(query);

        Assert.Equal(
            company.Id,
            repository.RequestedCompanyId);
        Assert.Equal(1, repository.GetByIdCallCount);

        Assert.Equal(company.Id, result.Id);
        Assert.Equal(company.Name, result.Name);
        Assert.Equal(company.TaxId, result.TaxId);
        Assert.Equal(
            company.ContactEmail,
            result.ContactEmail);
        Assert.Equal(
            company.IsActive,
            result.IsActive);
        Assert.Equal(
            company.CreatedAtUtc,
            result.CreatedAtUtc);
        Assert.Equal(
            company.UpdatedAtUtc,
            result.UpdatedAtUtc);
    }

    [Fact]
    public async Task HandleAsyncWithMissingCompanyThrowsNotFoundException()
    {
        Guid companyId = Guid.NewGuid();

        var repository =
            new CompanyRepositoryStub(null);

        var handler =
            new GetCompanyByIdHandler(repository);

        var query =
            new GetCompanyByIdQuery(companyId);

        NotFoundException exception =
            await Assert.ThrowsAsync<NotFoundException>(
                () => handler.HandleAsync(query));

        Assert.Equal(
            "Company was not found.",
            exception.Message);
        Assert.Equal(
            companyId,
            repository.RequestedCompanyId);
        Assert.Equal(1, repository.GetByIdCallCount);
    }

    private static Company CreateCompany()
    {
        return new Company(
            "FlowDesk Tecnologia",
            "12.345.678/0001-95",
            "contact@flowdesk.com.br");
    }

    private sealed class CompanyRepositoryStub
        : ICompanyRepository
    {
        private readonly Company? _company;

        public CompanyRepositoryStub(
            Company? company)
        {
            _company = company;
        }

        public Guid? RequestedCompanyId { get; private set; }

        public int GetByIdCallCount { get; private set; }

        public Task<bool> ExistsByTaxIdAsync(
            string taxId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(false);
        }

        public Task<Company?> GetByIdAsync(
            Guid companyId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            RequestedCompanyId = companyId;
            GetByIdCallCount++;

            Company? result =
                _company?.Id == companyId
                    ? _company
                    : null;

            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<Company>> ListAsync(
            bool includeInactive,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult<IReadOnlyList<Company>>(
                Array.Empty<Company>());
        }

        public Task AddAsync(
            Company company,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }
    }
}
