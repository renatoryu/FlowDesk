using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Application.Common.Exceptions;
using FlowDesk.Domain.Entities;
using FluentValidation;

namespace FlowDesk.Application.Authentication.Refresh;

public sealed class RefreshSessionHandler
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IValidator<RefreshSessionCommand> _validator;

    public RefreshSessionHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IAccessTokenGenerator accessTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IValidator<RefreshSessionCommand> validator)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _accessTokenGenerator = accessTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _validator = validator;
    }

    public async Task<RefreshSessionResult> HandleAsync(
        RefreshSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(
            command,
            cancellationToken);

        string tokenHash =
            _refreshTokenGenerator.ComputeHash(
                command.RefreshToken);

        RefreshToken? currentRefreshToken =
            await _refreshTokenRepository.GetByTokenHashAsync(
                tokenHash,
                cancellationToken);

        DateTime utcNow = DateTime.UtcNow;

        if (currentRefreshToken is null ||
            !currentRefreshToken.IsActive(utcNow))
        {
            throw new UnauthorizedException(
                "Invalid or expired refresh token.");
        }

        User? user =
            await _userRepository.GetByIdAsync(
                currentRefreshToken.UserId,
                cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException(
                "Invalid or expired refresh token.");
        }

        AccessTokenResult accessToken =
            _accessTokenGenerator.Generate(user);

        GeneratedRefreshToken generatedRefreshToken =
            _refreshTokenGenerator.Generate();

        var newRefreshToken = new RefreshToken(
            user.Id,
            generatedRefreshToken.TokenHash,
            generatedRefreshToken.ExpiresAtUtc);

        currentRefreshToken.Revoke(
            utcNow,
            newRefreshToken.Id);

        await _refreshTokenRepository.AddAsync(
            newRefreshToken,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new RefreshSessionResult(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            generatedRefreshToken.Token,
            generatedRefreshToken.ExpiresAtUtc);
    }
}
