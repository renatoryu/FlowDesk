using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowDesk.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration
    : IEntityTypeConfiguration<Category>
{
    private static readonly DateTime SeedTimestampUtc =
        new(
            2026,
            8,
            6,
            0,
            0,
            0,
            DateTimeKind.Utc);

    public void Configure(
        EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(category => category.Id);

        builder
            .Property(category => category.Id)
            .ValueGeneratedNever();

        builder
            .Property(category => category.Name)
            .HasMaxLength(Category.MaxNameLength)
            .IsRequired();

        builder
            .Property(category => category.NormalizedName)
            .HasMaxLength(Category.MaxNameLength)
            .IsRequired();

        builder
            .HasIndex(category => category.NormalizedName)
            .IsUnique()
            .HasDatabaseName(
                "UX_Categories_NormalizedName");

        builder
            .Property(category => category.Description)
            .HasMaxLength(Category.MaxDescriptionLength);

        builder
            .Property(category => category.IsActive)
            .IsRequired();

        builder
            .Property(category => category.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder
            .Property(category => category.UpdatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder
            .Property<byte[]>("RowVersion")
            .IsRowVersion()
            .IsRequired();

        builder.HasData(
            CreateSeed(
                "11111111-1111-1111-1111-111111111111",
                "General",
                "General service requests."),

            CreateSeed(
                "22222222-2222-2222-2222-222222222222",
                "Access",
                "Authentication and permission problems."),

            CreateSeed(
                "33333333-3333-3333-3333-333333333333",
                "Hardware",
                "Physical equipment and device problems."),

            CreateSeed(
                "44444444-4444-4444-4444-444444444444",
                "Software",
                "Application and software problems."),

            CreateSeed(
                "55555555-5555-5555-5555-555555555555",
                "Network",
                "Connectivity and network problems."));
    }

    private static object CreateSeed(
        string id,
        string name,
        string description)
    {
        return new
        {
            Id = Guid.Parse(id),
            Name = name,
            NormalizedName =
                name.ToUpperInvariant(),
            Description = description,
            IsActive = true,
            CreatedAtUtc = SeedTimestampUtc,
            UpdatedAtUtc = SeedTimestampUtc
        };
    }
}
