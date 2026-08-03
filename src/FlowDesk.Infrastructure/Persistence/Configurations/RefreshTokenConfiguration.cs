using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowDesk.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(
        EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(refreshToken => refreshToken.Id);

        builder
            .Property(refreshToken => refreshToken.Id)
            .ValueGeneratedNever();

        builder
            .Property(refreshToken => refreshToken.UserId)
            .IsRequired();

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(refreshToken => refreshToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(refreshToken => refreshToken.TokenHash)
            .HasMaxLength(RefreshToken.MaxTokenHashLength)
            .IsRequired();

        builder
            .HasIndex(refreshToken => refreshToken.TokenHash)
            .IsUnique();

        builder
            .Property(refreshToken => refreshToken.ExpiresAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder
            .Property(refreshToken => refreshToken.RevokedAtUtc)
            .HasColumnType("datetime2");

        builder
            .Property(refreshToken =>
                refreshToken.ReplacedByTokenId);

        builder
            .Property(refreshToken => refreshToken.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder
            .Property(refreshToken => refreshToken.UpdatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder
            .Property<byte[]>("RowVersion")
            .IsRowVersion()
            .IsRequired();
    }
}
