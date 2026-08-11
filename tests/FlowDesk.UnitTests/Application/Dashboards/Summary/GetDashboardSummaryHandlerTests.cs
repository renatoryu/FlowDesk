using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Dashboards.Summary;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;

namespace FlowDesk.UnitTests.Application.Dashboards.Summary;

public sealed class GetDashboardSummaryHandlerTests
{
    private const string ValidTaxId =
        "12.345.678/0001-95";

    [Fact]
    public async Task HandleAsyncWithCustomerScopesAndMapsCounts()
    {
        Company company = CreateCompany();

        User customer = CreateUser();
        customer.AssignToCompany(company.Id);

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var dashboardRepository = new DashboardRepositorySpy
        {
            Counts = new DashboardTicketCounts(
                4,
                3,
                7)
        };

        GetDashboardSummaryHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            dashboardRepository);

        DashboardSummaryResult result =
            await handler.HandleAsync(
                new GetDashboardSummaryQuery());

        DashboardTicketFilter filter =
            Assert.IsType<DashboardTicketFilter>(
                dashboardRepository.Filter);

        Assert.Equal(company.Id, filter.CompanyId);
        Assert.Equal(customer.Id, filter.RequesterId);
        Assert.Equal(4, result.OpenTickets);
        Assert.Equal(3, result.InProgressTickets);
        Assert.Equal(7, result.CompletedTickets);
        Assert.Equal(1, companyRepository.GetByIdCallCount);
        Assert.Equal(
            1,
            dashboardRepository.GetTicketCountsCallCount);
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Agent)]
    public async Task HandleAsyncWithPrivilegedRoleDoesNotApplyTenantScopeAndReturnsZeroCounts(
        UserRole role)
    {
        User user = CreateUser(role);

        var userRepository = new UserRepositorySpy
        {
            User = user
        };

        var companyRepository = new CompanyRepositorySpy();
        var dashboardRepository = new DashboardRepositorySpy();

        GetDashboardSummaryHandler handler = CreateHandler(
            new CurrentUserStub(user.Id, role),
            userRepository,
            companyRepository,
            dashboardRepository);

        DashboardSummaryResult result =
            await handler.HandleAsync(
                new GetDashboardSummaryQuery());

        DashboardTicketFilter filter =
            Assert.IsType<DashboardTicketFilter>(
                dashboardRepository.Filter);

        Assert.Null(filter.CompanyId);
        Assert.Null(filter.RequesterId);
        Assert.Equal(0, result.OpenTickets);
        Assert.Equal(0, result.InProgressTickets);
        Assert.Equal(0, result.CompletedTickets);
        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(
            1,
            dashboardRepository.GetTicketCountsCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithUnknownUserThrowsUnauthorizedException()
    {
        var dashboardRepository = new DashboardRepositorySpy();

        GetDashboardSummaryHandler handler = CreateHandler(
            new CurrentUserStub(
                Guid.NewGuid(),
                UserRole.Customer),
            new UserRepositorySpy(),
            new CompanyRepositorySpy(),
            dashboardRepository);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.HandleAsync(
                new GetDashboardSummaryQuery()));

        Assert.Equal(
            0,
            dashboardRepository.GetTicketCountsCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInactiveUserThrowsUnauthorizedException()
    {
        User customer = CreateUser();
        customer.Deactivate();

        var dashboardRepository = new DashboardRepositorySpy();

        GetDashboardSummaryHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            new UserRepositorySpy
            {
                User = customer
            },
            new CompanyRepositorySpy(),
            dashboardRepository);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.HandleAsync(
                new GetDashboardSummaryQuery()));

        Assert.Equal(
            0,
            dashboardRepository.GetTicketCountsCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithStaleRoleThrowsUnauthorizedException()
    {
        User customer = CreateUser();

        var dashboardRepository = new DashboardRepositorySpy();

        GetDashboardSummaryHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Agent),
            new UserRepositorySpy
            {
                User = customer
            },
            new CompanyRepositorySpy(),
            dashboardRepository);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.HandleAsync(
                new GetDashboardSummaryQuery()));

        Assert.Equal(
            0,
            dashboardRepository.GetTicketCountsCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithCustomerWithoutCompanyThrowsConflictException()
    {
        User customer = CreateUser();

        var companyRepository = new CompanyRepositorySpy();
        var dashboardRepository = new DashboardRepositorySpy();

        GetDashboardSummaryHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            new UserRepositorySpy
            {
                User = customer
            },
            companyRepository,
            dashboardRepository);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                new GetDashboardSummaryQuery()));

        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(
            0,
            dashboardRepository.GetTicketCountsCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithUnavailableCompanyThrowsConflictException()
    {
        User customer = CreateUser();
        customer.AssignToCompany(Guid.NewGuid());

        var companyRepository = new CompanyRepositorySpy();
        var dashboardRepository = new DashboardRepositorySpy();

        GetDashboardSummaryHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            new UserRepositorySpy
            {
                User = customer
            },
            companyRepository,
            dashboardRepository);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                new GetDashboardSummaryQuery()));

        Assert.Equal(1, companyRepository.GetByIdCallCount);
        Assert.Equal(
            0,
            dashboardRepository.GetTicketCountsCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInactiveCompanyThrowsConflictException()
    {
        Company company = CreateCompany();
        company.Deactivate();

        User customer = CreateUser();
        customer.AssignToCompany(company.Id);

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var dashboardRepository = new DashboardRepositorySpy();

        GetDashboardSummaryHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            new UserRepositorySpy
            {
                User = customer
            },
            companyRepository,
            dashboardRepository);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                new GetDashboardSummaryQuery()));

        Assert.Equal(1, companyRepository.GetByIdCallCount);
        Assert.Equal(
            0,
            dashboardRepository.GetTicketCountsCallCount);
    }

    private static GetDashboardSummaryHandler CreateHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        IDashboardRepository dashboardRepository)
    {
        return new GetDashboardSummaryHandler(
            currentUser,
            userRepository,
            companyRepository,
            dashboardRepository);
    }

    private static User CreateUser(
        UserRole role = UserRole.Customer)
    {
        return new User(
            "Ana Silva",
            "ana@example.com",
            "hashed-password",
            role);
    }

    private static Company CreateCompany()
    {
        return new Company(
            "FlowDesk Tecnologia",
            ValidTaxId,
            "contact@flowdesk.com.br");
    }

    private sealed class CurrentUserStub : ICurrentUser
    {
        public CurrentUserStub(
            Guid userId,
            UserRole role)
        {
            UserId = userId;
            Role = role;
        }

        public Guid UserId { get; }

        public UserRole Role { get; }
    }

    private sealed class UserRepositorySpy : IUserRepository
    {
        public User? User { get; init; }

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

        public Task<User?> GetForUpdateAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<User?>(null);
        }

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<User?> GetByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(User);
        }
    }

    private sealed class CompanyRepositorySpy : ICompanyRepository
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

    private sealed class DashboardRepositorySpy
        : IDashboardRepository
    {
        public DashboardTicketCounts Counts { get; init; } =
            new DashboardTicketCounts(
                0,
                0,
                0);

        public DashboardTicketFilter? Filter { get; private set; }

        public int GetTicketCountsCallCount { get; private set; }

        public Task<DashboardTicketCounts> GetTicketCountsAsync(
            DashboardTicketFilter filter,
            CancellationToken cancellationToken = default)
        {
            Filter = filter;
            GetTicketCountsCallCount++;

            return Task.FromResult(Counts);
        }
    }
}
