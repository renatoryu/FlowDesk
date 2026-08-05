using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowDesk.Infrastructure.Persistence.Configurations;

public sealed class CompanyConfiguration
    : IEntityTypeConfiguration<Company>
{
    public void Configure(
        EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(company => company.Id);

        builder
            .Property(company => company.Id)
            .ValueGeneratedNever();

        builder
            .Property(company => company.Name)
            .HasMaxLength(Company.MaxNameLength)
            .IsRequired();

        builder
            .Property(company => company.TaxId)
            .HasMaxLength(Company.TaxIdLength)
            .IsUnicode(false)
            .IsFixedLength()
            .IsRequired();

        builder
            .HasIndex(company => company.TaxId)
            .IsUnique();

        builder
            .Property(company => company.ContactEmail)
            .HasMaxLength(
                Company.MaxContactEmailLength)
            .IsRequired();

        builder
            .Property(company => company.IsActive)
            .IsRequired();

        builder
            .Property(company => company.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder
            .Property(company => company.UpdatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder
            .Property<byte[]>("RowVersion")
            .IsRowVersion()
            .IsRequired();
    }
}
