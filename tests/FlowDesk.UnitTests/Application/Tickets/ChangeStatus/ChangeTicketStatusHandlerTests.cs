using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Common.Models;
using FlowDesk.Application.Tickets.ChangeStatus;
using FlowDesk.Domain.Common;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation;

namespace FlowDesk.UnitTests.Application.Tickets.ChangeStatus;

public sealed class ChangeTicketStatusHandlerTests
{
    private const string ValidTaxId =
        "12.345.678/0001-95";

    [Fact]
    public async Task HandleAsyncWithOwnerCustomerClosesResolvedTicket()
    {
        Company company = CreateCompany();

        User customer = CreateUser();
        customer.AssignToCompany(company.Id);

        Ticket ticket = CreateTicket(company, customer);
        ticket.ChangeStatus(TicketStatus.InProgress);
        ticket.ChangeStatus(TicketStatus.Resolved);

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

        var unitOfWork = new UnitOfWorkSpy();

        ChangeTicketStatusHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            unitOfWork);

        ChangeTicketStatusResult result =
            await handler.HandleAsync(
                new ChangeTicketStatusCommand(
                    ticket.Id,
                    TicketStatus.Closed));

        Assert.Equal(TicketStatus.Closed, ticket.Status);
        Assert.NotNull(ticket.ResolvedAtUtc);
        Assert.NotNull(ticket.ClosedAtUtc);

        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal(TicketStatus.Closed, result.Status);
        Assert.Equal(ticket.ResolvedAtUtc, result.ResolvedAtUtc);
        Assert.Equal(ticket.ClosedAtUtc, result.ClosedAtUtc);

        Assert.Equal(1, companyRepository.GetByIdCallCount);
        Assert.Equal(1, ticketRepository.GetForUpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    [Theory]
    [InlineData(TicketStatus.InProgress)]
    [InlineData(TicketStatus.Resolved)]
    public async Task HandleAsyncWithCustomerChangingRestrictedStatusThrowsForbiddenException(
        TicketStatus status)
    {
        Company company = CreateCompany();

        User customer = CreateUser();
        customer.AssignToCompany(company.Id);

        Ticket ticket = CreateTicket(company, customer);

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

        var unitOfWork = new UnitOfWorkSpy();

        ChangeTicketStatusHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.HandleAsync(
                new ChangeTicketStatusCommand(
                    ticket.Id,
                    status)));

        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Agent)]
    public async Task HandleAsyncWithPrivilegedRoleChangesAnyTicketStatus(
        UserRole role)
    {
        User privilegedUser = CreateUser(role);
        User requester = CreateUser();

        Company company = CreateCompany();
        Ticket ticket = CreateTicket(company, requester);

        var userRepository = new UserRepositorySpy
        {
            User = privilegedUser
        };

        var companyRepository = new CompanyRepositorySpy();

        var ticketRepository = new TicketRepositorySpy
        {
            Ticket = ticket
        };

        var unitOfWork = new UnitOfWorkSpy();

        ChangeTicketStatusHandler handler = CreateHandler(
            new CurrentUserStub(
                privilegedUser.Id,
                role),
            userRepository,
            companyRepository,
            ticketRepository,
            unitOfWork);

        ChangeTicketStatusResult result =
            await handler.HandleAsync(
                new ChangeTicketStatusCommand(
                    ticket.Id,
                    TicketStatus.InProgress));

        Assert.Equal(TicketStatus.InProgress, ticket.Status);
        Assert.Equal(TicketStatus.InProgress, result.Status);

        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(1, ticketRepository.GetForUpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidCommandDoesNotAccessPersistence()
    {
        var currentUser = new CurrentUserStub(
            Guid.NewGuid(),
            UserRole.Customer);

        var userRepository = new UserRepositorySpy();
        var companyRepository = new CompanyRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        ChangeTicketStatusHandler handler = CreateHandler(
            currentUser,
            userRepository,
            companyRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(
                new ChangeTicketStatusCommand(
                    Guid.Empty,
                    (TicketStatus)0)));

        Assert.Equal(0, currentUser.UserIdReadCount);
        Assert.Equal(0, userRepository.GetByIdCallCount);
        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.GetForUpdateCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
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
        var unitOfWork = new UnitOfWorkSpy();

        ChangeTicketStatusHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new ChangeTicketStatusCommand(
                    Guid.NewGuid(),
                    TicketStatus.Closed)));

        Assert.Equal(1, ticketRepository.GetForUpdateCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithAnotherRequesterTicketThrowsNotFoundException()
    {
        Company company = CreateCompany();

        User customer = CreateUser();
        customer.AssignToCompany(company.Id);

        User anotherCustomer = CreateUser();
        Ticket ticket = CreateTicket(company, anotherCustomer);

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

        var unitOfWork = new UnitOfWorkSpy();

        ChangeTicketStatusHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new ChangeTicketStatusCommand(
                    ticket.Id,
                    TicketStatus.Closed)));

        Assert.Equal(1, ticketRepository.GetForUpdateCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidAgentTransitionThrowsDomainRuleException()
    {
        User agent = CreateUser(UserRole.Agent);
        User requester = CreateUser();

        Company company = CreateCompany();
        Ticket ticket = CreateTicket(company, requester);

        var userRepository = new UserRepositorySpy
        {
            User = agent
        };

        var companyRepository = new CompanyRepositorySpy();

        var ticketRepository = new TicketRepositorySpy
        {
            Ticket = ticket
        };

        var unitOfWork = new UnitOfWorkSpy();

        ChangeTicketStatusHandler handler = CreateHandler(
            new CurrentUserStub(
                agent.Id,
                UserRole.Agent),
            userRepository,
            companyRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<DomainRuleException>(
            () => handler.HandleAsync(
                new ChangeTicketStatusCommand(
                    ticket.Id,
                    TicketStatus.Resolved)));

        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithCustomerClosingOpenTicketThrowsDomainRuleException()
    {
        Company company = CreateCompany();

        User customer = CreateUser();
        customer.AssignToCompany(company.Id);

        Ticket ticket = CreateTicket(company, customer);

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

        var unitOfWork = new UnitOfWorkSpy();

        ChangeTicketStatusHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<DomainRuleException>(
            () => handler.HandleAsync(
                new ChangeTicketStatusCommand(
                    ticket.Id,
                    TicketStatus.Closed)));

        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    private static ChangeTicketStatusHandler CreateHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork)
    {
        return new ChangeTicketStatusHandler(
            currentUser,
            userRepository,
            companyRepository,
            ticketRepository,
            unitOfWork,
            new ChangeTicketStatusCommandValidator());
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

    private static Ticket CreateTicket(
        Company company,
        User requester)
    {
        return new Ticket(
            company.Id,
            Guid.NewGuid(),
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

        public Guid UserId
        {
            get
            {
                UserIdReadCount++;
                return _userId;
            }
        }

        public UserRole Role => _role;
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

    private sealed class TicketRepositorySpy : ITicketRepository
    {
        public Ticket? Ticket { get; init; }

        public int GetForUpdateCallCount { get; private set; }

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
            GetForUpdateCallCount++;

            return Task.FromResult(Ticket);
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
