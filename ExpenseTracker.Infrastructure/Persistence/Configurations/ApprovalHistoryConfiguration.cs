using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Persistence.Configurations;

public class ApprovalHistoryConfiguration : IEntityTypeConfiguration<ApprovalHistory>
{
    public void Configure(EntityTypeBuilder<ApprovalHistory> builder)
    {
        builder.ToTable("ApprovalHistory");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Action)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(h => h.Comment)
            .HasMaxLength(1000);

        // Actor (the user who performed the action).
        // Marked as optional from EF's perspective so the audit trail survives
        // even when the original actor is soft-deleted. ActorId remains required
        // (every audit row must say WHO did something) — but the navigation
        // property may legitimately be empty after a soft-delete.
        builder.HasOne(h => h.Actor)
            .WithMany()
            .HasForeignKey(h => h.ActorId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(h => h.ExpenseId);
        builder.HasIndex(h => h.ActorId);
        builder.HasIndex(h => h.CreatedAt);

        // No soft-delete on audit trail — audit logs are append-only.
    }
}