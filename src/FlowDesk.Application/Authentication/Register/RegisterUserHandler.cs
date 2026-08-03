using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.Application.Authentication.Register;

public sealed class RegisterUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<RegisterUserCommand> _validator;

    public RegisterUserHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IValidator<RegisterUserCommand> validator)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _validator = validator;
    }

    public async Task<RegisterUserResult> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        string normalizedEmail =
            command.Email.Trim().ToLowerInvariant();

        bool emailAlreadyExists =
            await _userRepository.ExistsByEmailAsync(
                normalizedEmail,
                cancellationToken);

        if (emailAlreadyExists)
        {
            throw new ConflictException(
                "A user with this email is already registered.");
        }

        string passwordHash =
            _passwordHasher.Hash(command.Password);

        var user = new User(
            command.FullName,
            normalizedEmail,
            passwordHash);

        await _userRepository.AddAsync(
            user,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new RegisterUserResult(
            user.Id,
            user.FullName,
            user.Email,
            user.Role.ToString());
    }
}
