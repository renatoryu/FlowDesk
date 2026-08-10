using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Common.Models;
using FlowDesk.Application.Tickets.Delete;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation;

namespace FlowDesk.UnitTests.Application.Tickets.Delete;

public sealed class DeleteTicketHandlerTests
{
    private const string ValidTaxId =
        "12.345.678/0001-95";

    [Fact]
    public async Task HandleAsyncWithOwnerCustomerSoftDeletesTicket()
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

        DeleteTicketHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            unitOfWork);

        await handler.HandleAsync(
            new DeleteTicketCommand(ticket.Id));

        Assert.True(ticket.IsDeleted);
        Assert.NotNull(ticket.DeletedAtUtc);
        Assert.Equal(customer.Id, ticket.DeletedByUserId);

        Assert.Equal(1, companyRepository.GetByIdCallCount);
        Assert.Equal(1, ticketRepository.GetForUpdateCallCount);
        Assert.True(ticketRepository.LastIncludeDeleted);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Agent)]
    public async Task HandleAsyncWithPrivilegedRoleDeletesAnyTicket(
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

        DeleteTicketHandler handler = CreateHandler(
            new CurrentUserStub(
                privilegedUser.Id,
                role),
            userRepository,
            companyRepository,
            ticketRepository,
            unitOfWork);

        await handler.HandleAsync(
            new DeleteTicketCommand(ticket.Id));

        Assert.True(ticket.IsDeleted);
        Assert.Equal(privilegedUser.Id, ticket.DeletedByUserId);

        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(1, ticketRepository.GetForUpdateCallCount);
        Assert.True(ticketRepository.LastIncludeDeleted);
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

        DeleteTicketHandler handler = CreateHandler(
            currentUser,
            userRepository,
            companyRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(
                new DeleteTicketCommand(Guid.Empty)));

        Assert.Equal(0, currentUser.UserIdReadCount);
        Assert.Equal(0, userRepository.GetByIdCallCount);
        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, ticketRepository.GetForUpdateCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
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
        var unitOfWork = new UnitOfWorkSpy();

        DeleteTicketHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                new DeleteTicketCommand(Guid.NewGuid())));

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

        DeleteTicketHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new DeleteTicketCommand(Guid.NewGuid())));

        Assert.Equal(1, ticketRepository.GetForUpdateCallCount);
        Assert.True(ticketRepository.LastIncludeDeleted);
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

        DeleteTicketHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new DeleteTicketCommand(ticket.Id)));

        Assert.Equal(1, ticketRepository.GetForUpdateCallCount);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithAlreadyDeletedTicketDoesNotSaveAgain()
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

        DeleteTicketHandler handler = CreateHandler(
            new CurrentUserStub(
                customer.Id,
                UserRole.Customer),
            userRepository,
            companyRepository,
            ticketRepository,
            unitOfWork);

        var command = new DeleteTicketCommand(ticket.Id);

        await handler.HandleAsync(command);
        await handler.HandleAsync(command);

        Assert.True(ticket.IsDeleted);
        Assert.Equal(2, ticketRepository.GetForUpdateCallCount);
        Assert.True(ticketRepository.LastIncludeDeleted);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    private static DeleteTicketHandler CreateHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork)
    {
        return new DeleteTicketHandler(
            currentUser,
            userRepository,
            companyRepository,
            ticketRepository,
            unitOfWork,
            new DeleteTicketCommandValidator());
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

        public bool LastIncludeDeleted { get; private set; }

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
            LastIncludeDeleted = includeDeleted;

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
