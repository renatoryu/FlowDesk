using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Users.AssignCompany;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation;

namespace FlowDesk.UnitTests.Application.Users.AssignCompany;

public sealed class AssignUserCompanyHandlerTests
{
    private const string ValidTaxId =
        "12.345.678/0001-95";

    [Fact]
    public async Task HandleAsyncWithValidCommandAssignsCustomerToCompany()
    {
        User user = CreateCustomer();
        Company company = CreateCompany();

        var userRepository = new UserRepositorySpy
        {
            UserForUpdate = user
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var unitOfWork = new UnitOfWorkSpy();

        AssignUserCompanyHandler handler = CreateHandler(
            userRepository,
            companyRepository,
            unitOfWork);

        AssignUserCompanyResult result =
            await handler.HandleAsync(
                new AssignUserCompanyCommand(
                    user.Id,
                    company.Id));

        Assert.Equal(company.Id, user.CompanyId);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(company.Id, result.CompanyId);
        Assert.Equal(1, userRepository.GetForUpdateCallCount);
        Assert.Equal(1, companyRepository.GetByIdCallCount);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidCommandDoesNotAccessPersistence()
    {
        var userRepository = new UserRepositorySpy();
        var companyRepository = new CompanyRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        AssignUserCompanyHandler handler = CreateHandler(
            userRepository,
            companyRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(
                new AssignUserCompanyCommand(
                    Guid.Empty,
                    Guid.NewGuid())));

        Assert.Equal(0, userRepository.GetForUpdateCallCount);
        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithNonCustomerThrowsConflictException()
    {
        User user = CreateCustomer();
        user.ChangeRole(UserRole.Agent);

        var userRepository = new UserRepositorySpy
        {
            UserForUpdate = user
        };

        var companyRepository = new CompanyRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        AssignUserCompanyHandler handler = CreateHandler(
            userRepository,
            companyRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                new AssignUserCompanyCommand(
                    user.Id,
                    Guid.NewGuid())));

        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithExistingAssociationDoesNotSaveAgain()
    {
        User user = CreateCustomer();
        Company company = CreateCompany();

        user.AssignToCompany(company.Id);

        var userRepository = new UserRepositorySpy
        {
            UserForUpdate = user
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var unitOfWork = new UnitOfWorkSpy();

        AssignUserCompanyHandler handler = CreateHandler(
            userRepository,
            companyRepository,
            unitOfWork);

        await handler.HandleAsync(
            new AssignUserCompanyCommand(
                user.Id,
                company.Id));

        Assert.Equal(company.Id, user.CompanyId);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithUnknownUserThrowsNotFoundException()
    {
        var userRepository = new UserRepositorySpy();
        var companyRepository = new CompanyRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        AssignUserCompanyHandler handler = CreateHandler(
            userRepository,
            companyRepository,
            unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new AssignUserCompanyCommand(
                    Guid.NewGuid(),
                    Guid.NewGuid())));

        Assert.Equal(1, userRepository.GetForUpdateCallCount);
        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInactiveUserThrowsConflictException()
    {
        User user = CreateCustomer();
        user.Deactivate();

        var userRepository = new UserRepositorySpy
        {
            UserForUpdate = user
        };

        var companyRepository = new CompanyRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        AssignUserCompanyHandler handler = CreateHandler(
            userRepository,
            companyRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                new AssignUserCompanyCommand(
                    user.Id,
                    Guid.NewGuid())));

        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithUnknownCompanyThrowsNotFoundException()
    {
        User user = CreateCustomer();

        var userRepository = new UserRepositorySpy
        {
            UserForUpdate = user
        };

        var companyRepository = new CompanyRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        AssignUserCompanyHandler handler = CreateHandler(
            userRepository,
            companyRepository,
            unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new AssignUserCompanyCommand(
                    user.Id,
                    Guid.NewGuid())));

        Assert.Equal(1, companyRepository.GetByIdCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInactiveCompanyThrowsConflictException()
    {
        User user = CreateCustomer();
        Company company = CreateCompany();
        company.Deactivate();

        var userRepository = new UserRepositorySpy
        {
            UserForUpdate = user
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var unitOfWork = new UnitOfWorkSpy();

        AssignUserCompanyHandler handler = CreateHandler(
            userRepository,
            companyRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                new AssignUserCompanyCommand(
                    user.Id,
                    company.Id)));

        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    private static AssignUserCompanyHandler CreateHandler(
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork)
    {
        return new AssignUserCompanyHandler(
            userRepository,
            companyRepository,
            unitOfWork,
            new AssignUserCompanyCommandValidator());
    }

    private static User CreateCustomer()
    {
        return new User(
            "Ana Silva",
            "ana@example.com",
            "hashed-password");
    }

    private static Company CreateCompany()
    {
        return new Company(
            "FlowDesk Tecnologia",
            ValidTaxId,
            "contact@flowdesk.com.br");
    }

    private sealed class UserRepositorySpy : IUserRepository
    {
        public User? UserForUpdate { get; init; }

        public int GetForUpdateCallCount { get; private set; }

        public Task<bool> ExistsByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<User?>(null);
        }

        public Task<User?> GetByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<User?>(null);
        }

        public Task<User?> GetForUpdateAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            GetForUpdateCallCount++;

            return Task.FromResult(UserForUpdate);
        }

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class CompanyRepositorySpy
        : ICompanyRepository
    {
        public Company? Company { get; init; }

        public int GetByIdCallCount { get; private set; }

        public Task<bool> ExistsByTaxIdAsync(
            string taxId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<Company?> GetByIdAsync(
            Guid companyId,
            CancellationToken cancellationToken = default)
        {
            GetByIdCallCount++;

            return Task.FromResult(Company);
        }

        public Task<IReadOnlyList<Company>> ListAsync(
            bool includeInactive,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Company>>(
                Array.Empty<Company>());
        }

        public Task AddAsync(
            Company company,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class UnitOfWorkSpy : IUnitOfWork
    {
        public int SaveCallCount { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveCallCount++;

            return Task.FromResult(1);
        }
    }
}
