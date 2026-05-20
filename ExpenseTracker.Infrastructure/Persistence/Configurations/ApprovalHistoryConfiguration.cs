using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Identity;
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

        // Actor relationship — declared without a navigation property because User
        // lives in Infrastructure (so ApprovalHistory in Domain can't reference it
        // directly). EF still knows about the relationship via ActorId + HasOne<User>().
        // IsRequired(false) keeps the audit row alive if the actor is soft-deleted.
        builder.HasOne<User>()
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
