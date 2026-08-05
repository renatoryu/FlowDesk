using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Companies.List;
using FlowDesk.Domain.Entities;

namespace FlowDesk.UnitTests.Application.Companies.List;

public sealed class ListCompaniesHandlerTests
{
    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 2)]
    public async Task HandleAsyncForwardsFilterAndMapsCompanies(
        bool includeInactive,
        int expectedCount)
    {
        var activeCompany = new Company(
            "FlowDesk Tecnologia",
            "12.345.678/0001-95",
            "contact@flowdesk.com.br");

        var inactiveCompany = new Company(
            "Acme Serviços",
            "11.444.777/0001-61",
            "contact@acme.com.br");

        inactiveCompany.Deactivate();

        var repository = new CompanyRepositoryStub(
            new[] { activeCompany, inactiveCompany });

        var handler =
            new ListCompaniesHandler(repository);

        var query =
            new ListCompaniesQuery(includeInactive);

        IReadOnlyList<ListCompanyResult> result =
            await handler.HandleAsync(query);

        Assert.Equal(
            includeInactive,
            repository.RequestedIncludeInactive);
        Assert.Equal(1, repository.ListCallCount);
        Assert.Equal(expectedCount, result.Count);

        Assert.Equal(activeCompany.Id, result[0].Id);
        Assert.Equal(activeCompany.Name, result[0].Name);
        Assert.Equal(activeCompany.TaxId, result[0].TaxId);
        Assert.True(result[0].IsActive);

        if (includeInactive)
        {
            Assert.Equal(
                inactiveCompany.Id,
                result[1].Id);
            Assert.False(result[1].IsActive);
        }
    }

    [Fact]
    public async Task HandleAsyncWithNoCompaniesReturnsEmptyList()
    {
        var repository = new CompanyRepositoryStub(
            Array.Empty<Company>());

        var handler =
            new ListCompaniesHandler(repository);

        IReadOnlyList<ListCompanyResult> result =
            await handler.HandleAsync(
                new ListCompaniesQuery());

        Assert.Empty(result);
    }

    private sealed class CompanyRepositoryStub
        : ICompanyRepository
    {
        private readonly IReadOnlyList<Company> _companies;

        public CompanyRepositoryStub(
            IReadOnlyList<Company> companies)
        {
            _companies = companies;
        }

        public bool? RequestedIncludeInactive
        {
            get;
            private set;
        }

        public int ListCallCount { get; private set; }

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

            return Task.FromResult<Company?>(null);
        }

        public Task<IReadOnlyList<Company>> ListAsync(
            bool includeInactive,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            RequestedIncludeInactive = includeInactive;
            ListCallCount++;

            IReadOnlyList<Company> result =
                includeInactive
                    ? _companies
                    : _companies
                        .Where(company => company.IsActive)
                        .ToArray();

            return Task.FromResult(result);
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
