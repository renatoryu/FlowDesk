using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowDesk.Infrastructure.Persistence.Configurations;

public sealed class AttachmentConfiguration
    : IEntityTypeConfiguration<Attachment>
{
    public void Configure(
        EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable(
            "Attachments",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_Attachments_SizeInBytes",
                    $"[SizeInBytes] > 0 AND " +
                    $"[SizeInBytes] <= {Attachment.MaxFileSizeInBytes}");

                tableBuilder.HasCheckConstraint(
                    "CK_Attachments_ContentType",
                    "[ContentType] IN " +
                    "('application/pdf', 'image/png', 'image/jpeg')");
            });

        builder.HasKey(attachment => attachment.Id);

        builder
            .Property(attachment => attachment.Id)
            .ValueGeneratedNever();

        builder
            .Property(attachment => attachment.TicketId)
            .IsRequired();

        builder
            .Property(attachment => attachment.UploadedById)
            .IsRequired();

        builder
            .Property(attachment => attachment.OriginalFileName)
            .HasMaxLength(Attachment.MaxOriginalFileNameLength)
            .IsRequired();

        builder
            .Property(attachment => attachment.StoredFileName)
            .HasMaxLength(Attachment.MaxStoredFileNameLength)
            .IsUnicode(false)
            .IsRequired();

        builder
            .Property(attachment => attachment.ContentType)
            .HasMaxLength(Attachment.MaxContentTypeLength)
            .IsUnicode(false)
            .IsRequired();

        builder
            .Property(attachment => attachment.SizeInBytes)
            .IsRequired();

        builder
            .Property(attachment => attachment.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder
            .Property(attachment => attachment.UpdatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder
            .HasOne<Ticket>()
            .WithMany()
            .HasForeignKey(attachment => attachment.TicketId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(attachment => attachment.UploadedById)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasIndex(attachment => attachment.StoredFileName)
            .IsUnique();

        builder
            .HasIndex(attachment => new
            {
                attachment.TicketId,
                attachment.CreatedAtUtc,
                attachment.Id
            });

        builder
            .HasIndex(attachment => attachment.UploadedById);
    }
}
