using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Companies.Deactivate;
using FlowDesk.Domain.Entities;

namespace FlowDesk.UnitTests.Application.Companies.Deactivate;

public sealed class DeactivateCompanyHandlerTests
{
    [Fact]
    public async Task HandleAsyncWithActiveCompanyDeactivatesAndSaves()
    {
        Company company = CreateCompany();

        DateTime previousUpdatedAt =
            company.UpdatedAtUtc;

        var repository =
            new CompanyRepositoryStub(company);

        var unitOfWork =
            new UnitOfWorkSpy();

        var handler =
            new DeactivateCompanyHandler(
                repository,
                unitOfWork);

        await handler.HandleAsync(
            new DeactivateCompanyCommand(company.Id));

        Assert.False(company.IsActive);
        Assert.True(
            company.UpdatedAtUtc >= previousUpdatedAt);
        Assert.Equal(1, repository.GetByIdCallCount);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInactiveCompanyDoesNotSaveAgain()
    {
        Company company = CreateCompany();
        company.Deactivate();

        DateTime previousUpdatedAt =
            company.UpdatedAtUtc;

        var repository =
            new CompanyRepositoryStub(company);

        var unitOfWork =
            new UnitOfWorkSpy();

        var handler =
            new DeactivateCompanyHandler(
                repository,
                unitOfWork);

        await handler.HandleAsync(
            new DeactivateCompanyCommand(company.Id));

        Assert.False(company.IsActive);
        Assert.Equal(
            previousUpdatedAt,
            company.UpdatedAtUtc);
        Assert.Equal(1, repository.GetByIdCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithMissingCompanyThrowsNotFoundException()
    {
        var repository =
            new CompanyRepositoryStub(null);

        var unitOfWork =
            new UnitOfWorkSpy();

        var handler =
            new DeactivateCompanyHandler(
                repository,
                unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new DeactivateCompanyCommand(
                    Guid.NewGuid())));

        Assert.Equal(1, repository.GetByIdCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
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
