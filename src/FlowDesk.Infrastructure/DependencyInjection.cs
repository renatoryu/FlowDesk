using System.IdentityModel.Tokens.Jwt;
using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Security;
using FlowDesk.Infrastructure.Authentication;
using FlowDesk.Infrastructure.Persistence;
using FlowDesk.Infrastructure.Persistence.Repositories;
using FlowDesk.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace FlowDesk.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<FlowDeskDbContext>(
            options => options.UseSqlServer(connectionString));

        services.AddScoped<
            ICompanyRepository,
            CompanyRepository>();

        services.AddScoped<
            ICategoryRepository,
            CategoryRepository>();

        services.AddScoped<
            ITicketRepository,
            TicketRepository>();

        services.AddScoped<
            ICommentRepository,
            CommentRepository>();

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<
            IRefreshTokenRepository,
            RefreshTokenRepository>();

        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<FlowDeskDbContext>());

        services.AddSingleton<
            IPasswordHasher,
            AspNetCorePasswordHasher>();

        AddJwtAuthentication(services, configuration);

        return services;
    }

    private static void AddJwtAuthentication(
        IServiceCollection services,
        IConfiguration configuration)
    {
        JwtOptions jwtOptions =
            configuration
                .GetRequiredSection(JwtOptions.SectionName)
                .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "JWT configuration could not be loaded.");

        byte[] signingKeyBytes =
            ValidateJwtOptions(jwtOptions);

        var signingKey =
            new SymmetricSecurityKey(signingKeyBytes);

        services.AddSingleton(jwtOptions);

        services.AddSingleton<
            IAccessTokenGenerator,
            JwtTokenGenerator>();

        services.AddSingleton<
            IRefreshTokenGenerator,
            RefreshTokenGenerator>();

        services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,

                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,

                        NameClaimType =
                            JwtRegisteredClaimNames.Name,

                        RoleClaimType = "role",

                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
            });

        services.AddAuthorization();
    }

    private static byte[] ValidateJwtOptions(
        JwtOptions jwtOptions)
    {
        if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
        {
            throw new InvalidOperationException(
                "JWT issuer was not configured.");
        }

        if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
        {
            throw new InvalidOperationException(
                "JWT audience was not configured.");
        }

        if (jwtOptions.AccessTokenExpirationMinutes <= 0)
        {
            throw new InvalidOperationException(
                "JWT access token expiration must be positive.");
        }

        if (jwtOptions.RefreshTokenExpirationDays <= 0)
        {
            throw new InvalidOperationException(
                "JWT refresh token expiration must be positive.");
        }

        byte[] signingKeyBytes;

        try
        {
            signingKeyBytes =
                Convert.FromBase64String(jwtOptions.SigningKey);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException(
                "JWT signing key must be valid Base64.",
                exception);
        }

        if (signingKeyBytes.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT signing key must contain at least 256 bits.");
        }

        return signingKeyBytes;
    }
}
