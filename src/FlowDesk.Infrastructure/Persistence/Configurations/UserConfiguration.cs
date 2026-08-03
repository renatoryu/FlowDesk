using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowDesk.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder
            .Property(user => user.Id)
            .ValueGeneratedNever();

        builder
            .Property(user => user.FullName)
            .HasMaxLength(User.MaxFullNameLength)
            .IsRequired();

        builder
            .Property(user => user.Email)
            .HasMaxLength(User.MaxEmailLength)
            .IsRequired();

        builder
            .HasIndex(user => user.Email)
            .IsUnique();

        builder
            .Property(user => user.PasswordHash)
            .HasMaxLength(User.MaxPasswordHashLength)
            .IsRequired();

        builder
            .Property(user => user.Role)
            .HasConversion<int>()
            .IsRequired();

        builder
            .Property(user => user.IsActive)
            .IsRequired();

        builder
            .Property(user => user.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder
            .Property(user => user.UpdatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();
    }
}
