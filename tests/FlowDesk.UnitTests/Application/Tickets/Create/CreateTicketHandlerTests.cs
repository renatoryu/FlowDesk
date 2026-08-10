using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Application.Tickets.Create;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using FluentValidation;

namespace FlowDesk.UnitTests.Application.Tickets.Create;

public sealed class CreateTicketHandlerTests
{
    private const string ValidTaxId =
        "12.345.678/0001-95";

    [Fact]
    public async Task HandleAsyncWithValidCommandCreatesTicketForCurrentCustomer()
    {
        Company company = CreateCompany();
        User requester = CreateCustomer();
        requester.AssignToCompany(company.Id);

        Category category = CreateCategory();

        var currentUser = new CurrentUserStub(requester.Id);

        var userRepository = new UserRepositorySpy
        {
            User = requester
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var categoryRepository = new CategoryRepositorySpy
        {
            Category = category
        };

        var ticketRepository = new TicketRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateTicketHandler handler = CreateHandler(
            currentUser,
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork);

        CreateTicketResult result =
            await handler.HandleAsync(
                CreateValidCommand(category.Id));

        Ticket ticket = Assert.IsType<Ticket>(
            ticketRepository.AddedTicket);

        Assert.Equal(requester.Id, ticket.RequesterId);
        Assert.Equal(company.Id, ticket.CompanyId);
        Assert.Equal(category.Id, ticket.CategoryId);
        Assert.Equal(TicketPriority.High, ticket.Priority);
        Assert.Equal(TicketStatus.Open, ticket.Status);

        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal(ticket.RequesterId, result.RequesterId);
        Assert.Equal(ticket.CompanyId, result.CompanyId);
        Assert.Equal(ticket.Status, result.Status);

        Assert.Equal(1, currentUser.UserIdReadCount);
        Assert.Equal(1, userRepository.GetByIdCallCount);
        Assert.Equal(1, companyRepository.GetByIdCallCount);
        Assert.Equal(1, categoryRepository.GetByIdCallCount);
        Assert.Equal(1, ticketRepository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsyncWithInvalidCommandDoesNotAccessPersistence()
    {
        var currentUser = new CurrentUserStub(Guid.NewGuid());
        var userRepository = new UserRepositorySpy();
        var companyRepository = new CompanyRepositorySpy();
        var categoryRepository = new CategoryRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateTicketHandler handler = CreateHandler(
            currentUser,
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(
                new CreateTicketCommand(
                    Guid.Empty,
                    string.Empty,
                    string.Empty,
                    (TicketPriority)0)));

        Assert.Equal(0, currentUser.UserIdReadCount);
        Assert.Equal(0, userRepository.GetByIdCallCount);
        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, categoryRepository.GetByIdCallCount);
        AssertTicketWasNotSaved(
            ticketRepository,
            unitOfWork);
    }

    [Fact]
    public async Task HandleAsyncWithUnknownCurrentUserThrowsUnauthorizedException()
    {
        var userRepository = new UserRepositorySpy();
        var companyRepository = new CompanyRepositorySpy();
        var categoryRepository = new CategoryRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateTicketHandler handler = CreateHandler(
            new CurrentUserStub(Guid.NewGuid()),
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.HandleAsync(
                CreateValidCommand(Guid.NewGuid())));

        Assert.Equal(1, userRepository.GetByIdCallCount);
        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, categoryRepository.GetByIdCallCount);
        AssertTicketWasNotSaved(
            ticketRepository,
            unitOfWork);
    }

    [Fact]
    public async Task HandleAsyncWithInactiveCustomerThrowsUnauthorizedException()
    {
        User requester = CreateCustomer();
        requester.Deactivate();

        var userRepository = new UserRepositorySpy
        {
            User = requester
        };

        var companyRepository = new CompanyRepositorySpy();
        var categoryRepository = new CategoryRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateTicketHandler handler = CreateHandler(
            new CurrentUserStub(requester.Id),
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.HandleAsync(
                CreateValidCommand(Guid.NewGuid())));

        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, categoryRepository.GetByIdCallCount);
        AssertTicketWasNotSaved(
            ticketRepository,
            unitOfWork);
    }

    [Fact]
    public async Task HandleAsyncWithNonCustomerThrowsForbiddenException()
    {
        User requester = CreateCustomer();
        requester.ChangeRole(UserRole.Agent);

        var userRepository = new UserRepositorySpy
        {
            User = requester
        };

        var companyRepository = new CompanyRepositorySpy();
        var categoryRepository = new CategoryRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateTicketHandler handler = CreateHandler(
            new CurrentUserStub(requester.Id),
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.HandleAsync(
                CreateValidCommand(Guid.NewGuid())));

        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, categoryRepository.GetByIdCallCount);
        AssertTicketWasNotSaved(
            ticketRepository,
            unitOfWork);
    }

    [Fact]
    public async Task HandleAsyncWithCustomerWithoutCompanyThrowsConflictException()
    {
        User requester = CreateCustomer();

        var userRepository = new UserRepositorySpy
        {
            User = requester
        };

        var companyRepository = new CompanyRepositorySpy();
        var categoryRepository = new CategoryRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateTicketHandler handler = CreateHandler(
            new CurrentUserStub(requester.Id),
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                CreateValidCommand(Guid.NewGuid())));

        Assert.Equal(0, companyRepository.GetByIdCallCount);
        Assert.Equal(0, categoryRepository.GetByIdCallCount);
        AssertTicketWasNotSaved(
            ticketRepository,
            unitOfWork);
    }

    [Fact]
    public async Task HandleAsyncWithUnavailableCompanyThrowsConflictException()
    {
        User requester = CreateCustomer();
        requester.AssignToCompany(Guid.NewGuid());

        var userRepository = new UserRepositorySpy
        {
            User = requester
        };

        var companyRepository = new CompanyRepositorySpy();
        var categoryRepository = new CategoryRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateTicketHandler handler = CreateHandler(
            new CurrentUserStub(requester.Id),
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                CreateValidCommand(Guid.NewGuid())));

        Assert.Equal(1, companyRepository.GetByIdCallCount);
        Assert.Equal(0, categoryRepository.GetByIdCallCount);
        AssertTicketWasNotSaved(
            ticketRepository,
            unitOfWork);
    }

    [Fact]
    public async Task HandleAsyncWithInactiveCompanyThrowsConflictException()
    {
        Company company = CreateCompany();
        company.Deactivate();

        User requester = CreateCustomer();
        requester.AssignToCompany(company.Id);

        var userRepository = new UserRepositorySpy
        {
            User = requester
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var categoryRepository = new CategoryRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateTicketHandler handler = CreateHandler(
            new CurrentUserStub(requester.Id),
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                CreateValidCommand(Guid.NewGuid())));

        Assert.Equal(0, categoryRepository.GetByIdCallCount);
        AssertTicketWasNotSaved(
            ticketRepository,
            unitOfWork);
    }

    [Fact]
    public async Task HandleAsyncWithUnknownCategoryThrowsNotFoundException()
    {
        Company company = CreateCompany();

        User requester = CreateCustomer();
        requester.AssignToCompany(company.Id);

        var userRepository = new UserRepositorySpy
        {
            User = requester
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var categoryRepository = new CategoryRepositorySpy();
        var ticketRepository = new TicketRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateTicketHandler handler = CreateHandler(
            new CurrentUserStub(requester.Id),
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                CreateValidCommand(Guid.NewGuid())));

        Assert.Equal(1, categoryRepository.GetByIdCallCount);
        AssertTicketWasNotSaved(
            ticketRepository,
            unitOfWork);
    }

    [Fact]
    public async Task HandleAsyncWithInactiveCategoryThrowsConflictException()
    {
        Company company = CreateCompany();
        Category category = CreateCategory();
        category.Deactivate();

        User requester = CreateCustomer();
        requester.AssignToCompany(company.Id);

        var userRepository = new UserRepositorySpy
        {
            User = requester
        };

        var companyRepository = new CompanyRepositorySpy
        {
            Company = company
        };

        var categoryRepository = new CategoryRepositorySpy
        {
            Category = category
        };

        var ticketRepository = new TicketRepositorySpy();
        var unitOfWork = new UnitOfWorkSpy();

        CreateTicketHandler handler = CreateHandler(
            new CurrentUserStub(requester.Id),
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(
                CreateValidCommand(category.Id)));

        AssertTicketWasNotSaved(
            ticketRepository,
            unitOfWork);
    }

    private static CreateTicketHandler CreateHandler(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        ICategoryRepository categoryRepository,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork)
    {
        return new CreateTicketHandler(
            currentUser,
            userRepository,
            companyRepository,
            categoryRepository,
            ticketRepository,
            unitOfWork,
            new CreateTicketCommandValidator());
    }

    private static CreateTicketCommand CreateValidCommand(
        Guid categoryId)
    {
        return new CreateTicketCommand(
            categoryId,
            "Cannot access the system",
            "The user cannot sign in to the internal system.",
            TicketPriority.High);
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

    private static Category CreateCategory()
    {
        return new Category(
            "Access",
            "Authentication and permission problems.");
    }

    private static void AssertTicketWasNotSaved(
        TicketRepositorySpy ticketRepository,
        UnitOfWorkSpy unitOfWork)
    {
        Assert.Equal(0, ticketRepository.AddCallCount);
        Assert.Null(ticketRepository.AddedTicket);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    private sealed class CurrentUserStub : ICurrentUser
    {
        private readonly Guid _userId;

        public CurrentUserStub(Guid userId)
        {
            _userId = userId;
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

        public UserRole Role => UserRole.Customer;
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

    private sealed class CategoryRepositorySpy
        : ICategoryRepository
    {
        public Category? Category { get; init; }

        public int GetByIdCallCount { get; private set; }

        public Task<Category?> GetByIdAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default)
        {
            GetByIdCallCount++;

            return Task.FromResult(Category);
        }

        public Task<IReadOnlyList<Category>> ListActiveAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Category>>(
                Array.Empty<Category>());
        }
    }

    private sealed class TicketRepositorySpy
        : ITicketRepository
    {
        public Ticket? AddedTicket { get; private set; }

        public int AddCallCount { get; private set; }

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

        public Task AddAsync(
            Ticket ticket,
            CancellationToken cancellationToken = default)
        {
            AddedTicket = ticket;
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
            SaveCallCount++;

            return Task.FromResult(1);
        }
    }
}
