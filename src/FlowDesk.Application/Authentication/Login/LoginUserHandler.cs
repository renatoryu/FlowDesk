using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.Application.Authentication.Login;

public sealed class LoginUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IValidator<LoginUserCommand> _validator;

    public LoginUserHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IAccessTokenGenerator accessTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IValidator<LoginUserCommand> validator)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _accessTokenGenerator = accessTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
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

        AccessTokenResult accessToken =
            _accessTokenGenerator.Generate(user);

        GeneratedRefreshToken generatedRefreshToken =
            _refreshTokenGenerator.Generate();

        var refreshToken = new RefreshToken(
            user.Id,
            generatedRefreshToken.TokenHash,
            generatedRefreshToken.ExpiresAtUtc);

        await _refreshTokenRepository.AddAsync(
            refreshToken,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new LoginUserResult(
            user.Id,
            user.FullName,
            user.Email,
            user.Role.ToString(),
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            generatedRefreshToken.Token,
            generatedRefreshToken.ExpiresAtUtc);
    }
}
