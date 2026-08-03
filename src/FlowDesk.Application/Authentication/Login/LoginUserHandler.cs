using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.Application.Authentication.Login;

public sealed class LoginUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<LoginUserCommand> _validator;

    public LoginUserHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IValidator<LoginUserCommand> validator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _validator = validator;
    }

    public async Task<LoginUserResult> HandleAsync(
        LoginUserCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        string normalizedEmail =
            command.Email.Trim().ToLowerInvariant();

        User? user =
            await _userRepository.GetByEmailAsync(
                normalizedEmail,
                cancellationToken);

        if (user is null ||
            !user.IsActive ||
            !_passwordHasher.Verify(
                user.PasswordHash,
                command.Password))
        {
            throw new UnauthorizedException(
                "Invalid email or password.");
        }

        return new LoginUserResult(
            user.Id,
            user.FullName,
            user.Email,
            user.Role.ToString());
    }
}
