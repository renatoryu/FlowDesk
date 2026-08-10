using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Common.Models;
using FlowDesk.Application.Tickets.GetById;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation;

namespace FlowDesk.UnitTests.Application.Tickets.GetById;

public sealed class GetTicketByIdHandlerTests
{
    private const string ValidTaxId =
        "12.345.678/0001-95";

    [Fact]
    public async Task HandleAsyncWithOwnerCustomerReturnsTicketDetails()
    {
        Company company = CreateCompany();
        User customer = CreateUser();
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
            Ticket = ticket
        };

        GetTicketByIdHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository);

        GetTicketByIdResult result =
            await handler.HandleAsync(
                new GetTicketByIdQuery(ticket.Id));

        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal(ticket.CompanyId, result.CompanyId);
        Assert.Equal(ticket.CategoryId, result.CategoryId);
        Assert.Equal(ticket.RequesterId, result.RequesterId);
        Assert.Equal(ticket.Title, result.Title);
        Assert.Equal(ticket.Description, result.Description);
        Assert.Equal(ticket.Priority, result.Priority);
        Assert.Equal(ticket.Status, result.Status);
        Assert.Equal(ticket.CreatedAtUtc, result.CreatedAtUtc);
        Assert.Equal(ticket.UpdatedAtUtc, result.UpdatedAtUtc);
        Assert.Equal(
            ticket.StatusChangedAtUtc,
            result.StatusChangedAtUtc);

        Assert.Equal(1, companyRepository.GetByIdCallCount);
        Assert.Equal(1, ticketRepository.GetByIdCallCount);
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Agent)]
    public async Task HandleAsyncWithPrivilegedRoleReturnsAnyTicket(
        UserRole role)
    {
        User privilegedUser = CreateUser(role);
        User requester = CreateUser();
        Company company = CreateCompany();
        Category category = CreateCategory();

        Ticket ticket = CreateTicket(
            company,
            requester,
            category);

        var userRepository = new UserRepositorySpy
        {
            User = privilegedUser
        };

        var companyRepository = new CompanyRepositorySpy();

        var ticketRepository = new TicketRepositorySpy
        {
            Ticket = ticket
        };

        GetTicketByIdHandler handler = CreateHandler(
            new CurrentUserStub(
                privilegedUser.Id,
                role),
            userRepository,
            companyRepository,
            ticketRepository);

        GetTicketByIdResult result =
            await handler.HandleAsync(
                new GetTicketByIdQuery(ticket.Id));

        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal(ticket.Description, result.Description);
        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(1, ticketRepository.GetByIdCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithEmptyIdDoesNotAccessPersistence()
    {
        var currentUser = new CurrentUserStub(
            Guid.NewGuid(),
            UserRole.Customer);

        var userRepository = new UserRepositorySpy();
        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();

        GetTicketByIdHandler handler = CreateHandler(
            currentUser,
            userRepository,
            companyRepository,
            ticketRepository);

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(
                new GetTicketByIdQuery(Guid.Empty)));

        Assert.Equal(0, currentUser.UserIdReadCount);
        Assert.Equal(0, currentUser.RoleReadCount);
        Assert.Equal(0, userRepository.GetByIdCallCount);
        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.GetByIdCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithUnknownUserThrowsUnauthorizedException()
    {
        var userRepository = new UserRepositorySpy();
        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();

        GetTicketByIdHandler handler = CreateHandler(
            new CurrentUserStub(
                Guid.NewGuid(),
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.HandleAsync(
                new GetTicketByIdQuery(Guid.NewGuid())));

        Assert.Equal(1, userRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.GetByIdCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInactiveUserThrowsUnauthorizedException()
    {
        User customer = CreateUser();
        customer.Deactivate();

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();

        GetTicketByIdHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.HandleAsync(
                new GetTicketByIdQuery(Guid.NewGuid())));

        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.GetByIdCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithStaleRoleThrowsUnauthorizedException()
    {
        User customer = CreateUser();

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();

        GetTicketByIdHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Agent),
            userRepository,
            companyRepository,
            ticketRepository);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.HandleAsync(
                new GetTicketByIdQuery(Guid.NewGuid())));

        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.GetByIdCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithCustomerWithoutCompanyThrowsConflictException()
    {
        User customer = CreateUser();

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();

        GetTicketByIdHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                new GetTicketByIdQuery(Guid.NewGuid())));

        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.GetByIdCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithUnavailableCompanyThrowsConflictException()
    {
        User customer = CreateUser();
        customer.AssignToCompany(Guid.NewGuid());

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();

        GetTicketByIdHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                new GetTicketByIdQuery(Guid.NewGuid())));

        Assert.Equal(1, companyRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.GetByIdCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInactiveCompanyThrowsConflictException()
    {
        Company company = CreateCompany();
        company.Deactivate();

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

        var ticketRepository = new TicketRepositorySpy();

        GetTicketByIdHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                new GetTicketByIdQuery(Guid.NewGuid())));

        Assert.Equal(1, companyRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.GetByIdCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithAnotherRequesterTicketThrowsNotFoundException()
    {
        Company company = CreateCompany();

        User customer = CreateUser();
        customer.AssignToCompany(company.Id);

        User anotherCustomer = CreateUser();

        Category category = CreateCategory();

        Ticket ticket = CreateTicket(
            company,
            anotherCustomer,
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
            Ticket = ticket
        };

        GetTicketByIdHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new GetTicketByIdQuery(ticket.Id)));

        Assert.Equal(1, ticketRepository.GetByIdCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithTicketFromAnotherCompanyThrowsNotFoundException()
    {
        Company currentCompany = CreateCompany();
        Company previousCompany = new Company(
            "Empresa Anterior",
            "11.444.777/0001-61",
            "contact@previous.com.br");

        User customer = CreateUser();
        customer.AssignToCompany(currentCompany.Id);

        Category category = CreateCategory();

        Ticket ticket = CreateTicket(
            previousCompany,
            customer,
            category);

        var userRepository = new UserRepositorySpy
        {
            User = customer
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = currentCompany
        };

        var ticketRepository = new TicketRepositorySpy
        {
            Ticket = ticket
        };

        GetTicketByIdHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new GetTicketByIdQuery(ticket.Id)));

        Assert.Equal(1, ticketRepository.GetByIdCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithUnknownTicketThrowsNotFoundException()
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

        var ticketRepository = new TicketRepositorySpy();

        GetTicketByIdHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new GetTicketByIdQuery(Guid.NewGuid())));

        Assert.Equal(1, ticketRepository.GetByIdCallCount);
    }

    private static GetTicketByIdHandler CreateHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ITicketRepository ticketRepository)
    {
        return new GetTicketByIdHandler(
            currentUser,
            userRepository,
            companyRepository,
            ticketRepository,
            new GetTicketByIdQueryValidator());
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
        private readonly UserRole _role;

        public CurrentUserStub(
            Guid userId,
            UserRole role)
        {
            _userId = userId;
            _role = role;
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
        }
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
        public Ticket? Ticket { get; init; }

        public int GetByIdCallCount { get; private set; }

        public Task<Ticket?> GetByIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken = default)
        {
            GetByIdCallCount++;

            return Task.FromResult(Ticket);
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
            return Task.FromResult(
                new PagedResult<Ticket>(
                    Array.Empty<Ticket>(),
                    1,
                    20,
                    0));
        }

        public Task AddAsync(
            Ticket ticket,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
