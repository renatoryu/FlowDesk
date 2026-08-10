using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowDesk.Infrastructure.Persistence.Configurations;

public sealed class CommentConfiguration
    : IEntityTypeConfiguration<Comment>
{
    public void Configure(
        EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");

        builder.HasKey(comment => comment.Id);

        builder
            .Property(comment => comment.Id)
            .ValueGeneratedNever();

        builder
            .Property(comment => comment.TicketId)
            .IsRequired();

        builder
            .Property(comment => comment.AuthorId)
            .IsRequired();

        builder
            .Property(comment => comment.Content)
            .HasMaxLength(Comment.MaxContentLength)
            .IsRequired();

        builder
            .Property(comment => comment.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder
            .Property(comment => comment.UpdatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder
            .HasOne<Ticket>()
            .WithMany()
            .HasForeignKey(comment => comment.TicketId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(comment => comment.AuthorId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasIndex(comment => new
            {
                comment.TicketId,
                comment.CreatedAtUtc,
                comment.Id
            });
    }
}
