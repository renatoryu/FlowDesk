using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowDesk.Infrastructure.Persistence.Configurations;

public sealed class TicketConfiguration
    : IEntityTypeConfiguration<Ticket>
{
    public void Configure(
        EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable(
            "Tickets",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_Tickets_Priority",
                    "[Priority] IN (1, 2, 3, 4)");

                tableBuilder.HasCheckConstraint(
                    "CK_Tickets_Status",
                    "[Status] IN (1, 2, 3, 4)");

                tableBuilder.HasCheckConstraint(
                    "CK_Tickets_StatusDates",
                    "([Status] IN (1, 2) " +
                    "AND [ResolvedAtUtc] IS NULL " +
                    "AND [ClosedAtUtc] IS NULL) OR " +
                    "([Status] = 3 " +
                    "AND [ResolvedAtUtc] IS NOT NULL " +
                    "AND [ClosedAtUtc] IS NULL) OR " +
                    "([Status] = 4 " +
                    "AND [ResolvedAtUtc] IS NOT NULL " +
                    "AND [ClosedAtUtc] IS NOT NULL)");

                tableBuilder.HasCheckConstraint(
                    "CK_Tickets_LogicalDeletion",
                    "([IsDeleted] = 0 " +
                    "AND [DeletedAtUtc] IS NULL " +
                    "AND [DeletedByUserId] IS NULL) OR " +
                    "([IsDeleted] = 1 " +
                    "AND [DeletedAtUtc] IS NOT NULL " +
                    "AND [DeletedByUserId] IS NOT NULL)");
            });

        builder.HasKey(ticket => ticket.Id);

        builder
            .Property(ticket => ticket.Id)
            .ValueGeneratedNever();

        builder
            .Property(ticket => ticket.Title)
            .HasMaxLength(Ticket.MaxTitleLength)
            .IsRequired();

        builder
            .Property(ticket => ticket.Description)
            .HasMaxLength(Ticket.MaxDescriptionLength)
            .IsRequired();

        builder
            .Property(ticket => ticket.CompanyId)
            .IsRequired();

        builder
            .Property(ticket => ticket.CategoryId)
            .IsRequired();

        builder
            .Property(ticket => ticket.RequesterId)
            .IsRequired();

        builder
            .Property(ticket => ticket.Priority)
            .HasConversion<byte>()
            .HasColumnType("tinyint")
            .IsRequired();

        builder
            .Property(ticket => ticket.Status)
            .HasConversion<byte>()
            .HasColumnType("tinyint")
            .IsRequired();

        builder
            .Property(ticket => ticket.StatusChangedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder
            .Property(ticket => ticket.ResolvedAtUtc)
            .HasColumnType("datetime2");

        builder
            .Property(ticket => ticket.ClosedAtUtc)
            .HasColumnType("datetime2");

        builder
            .Property(ticket => ticket.IsDeleted)
            .IsRequired();

        builder
            .Property(ticket => ticket.DeletedAtUtc)
            .HasColumnType("datetime2");

        builder
            .Property(ticket => ticket.DeletedByUserId);

        builder
            .Property(ticket => ticket.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder
            .Property(ticket => ticket.UpdatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder
            .Property<byte[]>("RowVersion")
            .IsRowVersion()
            .IsRequired();

        builder
            .HasOne<Company>()
            .WithMany()
            .HasForeignKey(ticket => ticket.CompanyId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<Category>()
            .WithMany()
            .HasForeignKey(ticket => ticket.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(ticket => ticket.RequesterId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(ticket => ticket.DeletedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasIndex(ticket => new
            {
                ticket.CompanyId,
                ticket.Status,
                ticket.CreatedAtUtc
            })
            .HasFilter("[IsDeleted] = 0");

        builder
            .HasIndex(ticket => new
            {
                ticket.RequesterId,
                ticket.CreatedAtUtc
            })
            .HasFilter("[IsDeleted] = 0");

        builder
            .HasIndex(ticket => new
            {
                ticket.Status,
                ticket.CreatedAtUtc
            })
            .HasFilter("[IsDeleted] = 0");

        builder
            .HasIndex(ticket => ticket.CategoryId)
            .HasFilter("[IsDeleted] = 0");
    }
}
