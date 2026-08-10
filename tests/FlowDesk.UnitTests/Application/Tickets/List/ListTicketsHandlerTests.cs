using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Common.Models;
using FlowDesk.Application.Tickets.List;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation;

namespace FlowDesk.UnitTests.Application.Tickets.List;

public sealed class ListTicketsHandlerTests
{
    private const string ValidTaxId =
        "12.345.678/0001-95";

    [Fact]
    public async Task HandleAsyncWithCustomerScopesAndMapsPagedResult()
    {
        Company company = CreateCompany();

        User customer = CreateUser(UserRole.Customer);
        customer.AssignToCompany(company.Id);

        Category category = CreateCategory();

        Ticket ticket = CreateTicket(
            company,
            customer,
            category);

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var ticketRepository = new TicketRepositorySpy
        {
            Page = new PagedResult<Ticket>(
                new[] { ticket },
                2,
                5,
                11)
        };

        var currentUser = new CurrentUserStub(
            customer.Id,
            UserRole.Customer);

        ListTicketsHandler handler = CreateHandler(
            currentUser,
            userRepository,
            companyRepository,
            ticketRepository);

        ListTicketsResult result =
            await handler.HandleAsync(
                new ListTicketsQuery(
                    2,
                    5,
                    TicketStatus.Open,
                    TicketPriority.High,
                    category.Id));

        TicketListFilter filter = Assert.IsType<TicketListFilter>(
            ticketRepository.Filter);

        Assert.Equal(company.Id, filter.CompanyId);
        Assert.Equal(customer.Id, filter.RequesterId);
        Assert.Equal(category.Id, filter.CategoryId);
        Assert.Equal(TicketStatus.Open, filter.Status);
        Assert.Equal(TicketPriority.High, filter.Priority);
        Assert.Equal(2, filter.Page);
        Assert.Equal(5, filter.PageSize);

        TicketListItem item = Assert.Single(result.Items);

        Assert.Equal(ticket.Id, item.Id);
        Assert.Equal(ticket.CompanyId, item.CompanyId);
        Assert.Equal(ticket.RequesterId, item.RequesterId);
        Assert.Equal(ticket.Status, item.Status);
        Assert.Equal(2, result.Page);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(11, result.TotalCount);
        Assert.Equal(3, result.TotalPages);

        Assert.Equal(1, companyRepository.GetByIdCallCount);
        Assert.Equal(1, ticketRepository.ListCallCount);
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Agent)]
    public async Task HandleAsyncWithPrivilegedRoleDoesNotApplyTenantScope(
        UserRole role)
    {
        User user = CreateUser(role);

        var userRepository = new UserRepositorySpy
        {
            User = user
        };

        var companyRepository = new CompanyRepositorySpy();

        var ticketRepository = new TicketRepositorySpy
        {
            Page = new PagedResult<Ticket>(
                Array.Empty<Ticket>(),
                1,
                20,
                0)
        };

        ListTicketsHandler handler = CreateHandler(
            new CurrentUserStub(user.Id, role),
            userRepository,
            companyRepository,
            ticketRepository);

        Guid categoryId = Guid.NewGuid();

        await handler.HandleAsync(
            new ListTicketsQuery(
                1,
                20,
                TicketStatus.Open,
                TicketPriority.Medium,
                categoryId));

        TicketListFilter filter = Assert.IsType<TicketListFilter>(
            ticketRepository.Filter);

        Assert.Null(filter.CompanyId);
        Assert.Null(filter.RequesterId);
        Assert.Equal(categoryId, filter.CategoryId);
        Assert.Equal(TicketStatus.Open, filter.Status);
        Assert.Equal(TicketPriority.Medium, filter.Priority);
        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(1, ticketRepository.ListCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidQueryDoesNotAccessPersistence()
    {
        var currentUser = new CurrentUserStub(
            Guid.NewGuid(),
            UserRole.Customer);

        var userRepository = new UserRepositorySpy();
        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();

        ListTicketsHandler handler = CreateHandler(
            currentUser,
            userRepository,
            companyRepository,
            ticketRepository);

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(
                new ListTicketsQuery(
                    0,
                    101,
                    (TicketStatus)0,
                    (TicketPriority)0,
                    Guid.Empty)));

        Assert.Equal(0, currentUser.UserIdReadCount);
        Assert.Equal(0, currentUser.RoleReadCount);
        Assert.Equal(0, userRepository.GetByIdCallCount);
        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.ListCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithUnknownUserThrowsUnauthorizedException()
    {
        var userRepository = new UserRepositorySpy();
        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();

        ListTicketsHandler handler = CreateHandler(
            new CurrentUserStub(
                Guid.NewGuid(),
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.HandleAsync(
                new ListTicketsQuery()));

        Assert.Equal(1, userRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.ListCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInactiveUserThrowsUnauthorizedException()
    {
        User user = CreateUser(UserRole.Customer);
        user.Deactivate();

        var userRepository = new UserRepositorySpy
        {
            User = user
        };

        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();

        ListTicketsHandler handler = CreateHandler(
            new CurrentUserStub(
                user.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.HandleAsync(
                new ListTicketsQuery()));

        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.ListCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithStaleRoleThrowsUnauthorizedException()
    {
        User user = CreateUser(UserRole.Customer);

        var userRepository = new UserRepositorySpy
        {
            User = user
        };

        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();

        ListTicketsHandler handler = CreateHandler(
            new CurrentUserStub(
                user.Id,
                UserRole.Agent),
            userRepository,
            companyRepository,
            ticketRepository);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.HandleAsync(
                new ListTicketsQuery()));

        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.ListCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithCustomerWithoutCompanyThrowsConflictException()
    {
        User customer = CreateUser(UserRole.Customer);

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();

        ListTicketsHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                new ListTicketsQuery()));

        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.ListCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithUnavailableCompanyThrowsConflictException()
    {
        User customer = CreateUser(UserRole.Customer);
        customer.AssignToCompany(Guid.NewGuid());

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();

        ListTicketsHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                new ListTicketsQuery()));

        Assert.Equal(1, companyRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.ListCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInactiveCompanyThrowsConflictException()
    {
        Company company = CreateCompany();
        company.Deactivate();

        User customer = CreateUser(UserRole.Customer);
        customer.AssignToCompany(company.Id);

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var ticketRepository = new TicketRepositorySpy();

        ListTicketsHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                new ListTicketsQuery()));

        Assert.Equal(1, companyRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.ListCallCount);
    }

    private static ListTicketsHandler CreateHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ITicketRepository ticketRepository)
    {
        return new ListTicketsHandler(
            currentUser,
            userRepository,
            companyRepository,
            ticketRepository,
            new ListTicketsQueryValidator());
    }

    private static User CreateUser(UserRole role)
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

    private static Category CreateCategory()
    {
        return new Category(
            "Access",
            "Authentication and permission problems.");
    }

    private static Ticket CreateTicket(
        Company company,
        User requester,
        Category category)
    {
        return new Ticket(
            company.Id,
            category.Id,
            requester.Id,
            "Cannot access the system",
            "The user cannot sign in to the internal system.",
            TicketPriority.High);
    }

    private sealed class CurrentUserStub : ICurrentUser
    {
        private readonly Guid _userId;

        public CurrentUserStub(
            Guid userId,
            UserRole role)
        {
            _userId = userId;
            Role = role;
        }

        public int UserIdReadCount { get; private set; }

        public int RoleReadCount { get; private set; }

        public Guid UserId
        {
            get
            {
                UserIdReadCount++;
                return _userId;
            }
        }

        public UserRole Role
        {
            get
            {
                RoleReadCount++;
                return _role;
            }
            private init
            {
                _role = value;
            }
        }

        private readonly UserRole _role;
    }

    private sealed class UserRepositorySpy : IUserRepository
    {
        public User? User { get; init; }

        public int GetByIdCallCount { get; private set; }

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
            GetByIdCallCount++;

            return Task.FromResult(User);
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

    private sealed class TicketRepositorySpy
        : ITicketRepository
    {
        public PagedResult<Ticket> Page { get; init; } =
            new PagedResult<Ticket>(
                Array.Empty<Ticket>(),
                1,
                20,
                0);

        public TicketListFilter? Filter { get; private set; }

        public int ListCallCount { get; private set; }

        public Task<Ticket?> GetByIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Ticket?>(null);
        }

        public Task<Ticket?> GetForUpdateAsync(
            Guid ticketId,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Ticket?>(null);
        }

        public Task<PagedResult<Ticket>> ListAsync(
            TicketListFilter filter,
            CancellationToken cancellationToken = default)
        {
            Filter = filter;
            ListCallCount++;

            return Task.FromResult(Page);
        }

        public Task AddAsync(
            Ticket ticket,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
