using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Companies.Create;
using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.UnitTests.Application.Companies.Create;

public sealed class CreateCompanyHandlerTests
{
    private const string ValidTaxId =
        "12.345.678/0001-95";

    private const string NormalizedTaxId =
        "12345678000195";

    [Fact]
    public async Task HandleAsyncWithValidCommandAddsCompanyAndReturnsResult()
    {
        var repository = new CompanyRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateCompanyHandler handler =
            CreateHandler(repository, unitOfWork);

        var command = new CreateCompanyCommand(
            "  FlowDesk Tecnologia  ",
            ValidTaxId,
            "CONTACT@FLOWDESK.COM.BR");

        CreateCompanyResult result =
            await handler.HandleAsync(command);

        Assert.Equal(
            NormalizedTaxId,
            repository.CheckedTaxId);
        Assert.Equal(1, repository.ExistsCallCount);
        Assert.Equal(1, repository.AddCallCount);

        Company addedCompany =
            Assert.IsType<Company>(
                repository.AddedCompany);

        Assert.Equal(
            "FlowDesk Tecnologia",
            addedCompany.Name);
        Assert.Equal(
            NormalizedTaxId,
            addedCompany.TaxId);
        Assert.Equal(
            "contact@flowdesk.com.br",
            addedCompany.ContactEmail);

        Assert.Equal(1, unitOfWork.SaveCallCount);

        Assert.Equal(addedCompany.Id, result.Id);
        Assert.Equal(addedCompany.Name, result.Name);
        Assert.Equal(addedCompany.TaxId, result.TaxId);
        Assert.Equal(
            addedCompany.ContactEmail,
            result.ContactEmail);
        Assert.True(result.IsActive);
        Assert.Equal(
            addedCompany.CreatedAtUtc,
            result.CreatedAtUtc);
    }

    [Fact]
    public async Task HandleAsyncWithDuplicateTaxIdThrowsConflictException()
    {
        var repository = new CompanyRepositorySpy
        {
            TaxIdExists = true
        };

        var unitOfWork = new UnitOfWorkSpy();

        CreateCompanyHandler handler =
            CreateHandler(repository, unitOfWork);

        var command = new CreateCompanyCommand(
            "FlowDesk Tecnologia",
            ValidTaxId,
            "contact@flowdesk.com.br");

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(command));

        Assert.Equal(
            NormalizedTaxId,
            repository.CheckedTaxId);
        Assert.Equal(1, repository.ExistsCallCount);
        Assert.Equal(0, repository.AddCallCount);
        Assert.Null(repository.AddedCompany);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidCommandDoesNotAccessPersistence()
    {
        var repository = new CompanyRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateCompanyHandler handler =
            CreateHandler(repository, unitOfWork);

        var command = new CreateCompanyCommand(
            string.Empty,
            ValidTaxId,
            "contact@flowdesk.com.br");

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(command));

        Assert.Equal(0, repository.ExistsCallCount);
        Assert.Equal(0, repository.AddCallCount);
        Assert.Null(repository.CheckedTaxId);
        Assert.Null(repository.AddedCompany);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    private static CreateCompanyHandler CreateHandler(
        ICompanyRepository repository,
        IUnitOfWork unitOfWork)
    {
        return new CreateCompanyHandler(
            repository,
            unitOfWork,
            new CreateCompanyCommandValidator());
    }

    private sealed class CompanyRepositorySpy
        : ICompanyRepository
    {
        public bool TaxIdExists { get; init; }

        public string? CheckedTaxId { get; private set; }

        public Company? AddedCompany { get; private set; }

        public int ExistsCallCount { get; private set; }

        public int AddCallCount { get; private set; }

        public Task<bool> ExistsByTaxIdAsync(
            string taxId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CheckedTaxId = taxId;
            ExistsCallCount++;

            return Task.FromResult(TaxIdExists);
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

            return Task.FromResult<IReadOnlyList<Company>>(
                Array.Empty<Company>());
        }

        public Task AddAsync(
            Company company,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            AddedCompany = company;
            AddCallCount++;

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
