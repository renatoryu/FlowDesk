using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository
    : IRefreshTokenRepository
{
    private readonly FlowDeskDbContext _dbContext;

    public RefreshTokenRepository(
        FlowDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                refreshToken =>
                    refreshToken.TokenHash == tokenHash,
                cancellationToken);
    }

    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens.AddAsync(
            refreshToken,
            cancellationToken);
    }
}
