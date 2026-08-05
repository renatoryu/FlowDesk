using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Companies.Update;
using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.UnitTests.Application.Companies.Update;

public sealed class UpdateCompanyHandlerTests
{
    [Fact]
    public async Task HandleAsyncWithExistingCompanyUpdatesAndSaves()
    {
        Company company = CreateCompany();

        string originalTaxId = company.TaxId;
        DateTime previousUpdatedAt =
            company.UpdatedAtUtc;

        var repository =
            new CompanyRepositoryStub(company);

        var unitOfWork =
            new UnitOfWorkSpy();

        UpdateCompanyHandler handler =
            CreateHandler(repository, unitOfWork);

        var command = new UpdateCompanyCommand(
            company.Id,
            "  FlowDesk Support  ",
            "SUPPORT@FLOWDESK.COM.BR");

        UpdateCompanyResult result =
            await handler.HandleAsync(command);

        Assert.Equal(
            company.Id,
            repository.RequestedCompanyId);
        Assert.Equal(1, repository.GetByIdCallCount);

        Assert.Equal(
            "FlowDesk Support",
            company.Name);
        Assert.Equal(
            "support@flowdesk.com.br",
            company.ContactEmail);
        Assert.Equal(originalTaxId, company.TaxId);
        Assert.True(
            company.UpdatedAtUtc >= previousUpdatedAt);

        Assert.Equal(1, unitOfWork.SaveCallCount);

        Assert.Equal(company.Id, result.Id);
        Assert.Equal(company.Name, result.Name);
        Assert.Equal(company.TaxId, result.TaxId);
        Assert.Equal(
            company.ContactEmail,
            result.ContactEmail);
        Assert.Equal(
            company.UpdatedAtUtc,
            result.UpdatedAtUtc);
    }

    [Fact]
    public async Task HandleAsyncWithMissingCompanyThrowsNotFoundException()
    {
        var repository =
            new CompanyRepositoryStub(null);

        var unitOfWork =
            new UnitOfWorkSpy();

        UpdateCompanyHandler handler =
            CreateHandler(repository, unitOfWork);

        var command = new UpdateCompanyCommand(
            Guid.NewGuid(),
            "FlowDesk Support",
            "support@flowdesk.com.br");

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(command));

        Assert.Equal(1, repository.GetByIdCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidCommandDoesNotAccessPersistence()
    {
        Company company = CreateCompany();

        var repository =
            new CompanyRepositoryStub(company);

        var unitOfWork =
            new UnitOfWorkSpy();

        UpdateCompanyHandler handler =
            CreateHandler(repository, unitOfWork);

        var command = new UpdateCompanyCommand(
            Guid.Empty,
            string.Empty,
            "invalid-email");

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(command));

        Assert.Equal(0, repository.GetByIdCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    private static UpdateCompanyHandler CreateHandler(
        ICompanyRepository repository,
        IUnitOfWork unitOfWork)
    {
        return new UpdateCompanyHandler(
            repository,
            unitOfWork,
            new UpdateCompanyCommandValidator());
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

    private sealed class UnitOfWorkSpy : IUnitOfWork
    {
        public int SaveCallCount { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            SaveCallCount++;

            return Task.FromResult(1);
        }
    }
}
