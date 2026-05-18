using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Persistence.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.ReceiptBlobUrl)
            .HasMaxLength(2000);  // URLs can get long

        builder.Property(e => e.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        // Category relationship
        builder.HasOne(e => e.Category)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

       // ApprovalHistory: one expense → many history entries.
        // The audit relationship is marked optional from EF's perspective for the
        // same reason as the User→ApprovalHistory link: the audit trail must survive
        // even if its parent entity is soft-deleted. The FK remains NOT NULL — only
        // the C# navigation property is treated as optional in query planning.
        builder.HasMany(e => e.ApprovalHistory)
            .WithOne(h => h.Expense!)
            .HasForeignKey(h => h.ExpenseId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for the queries we'll write in Phase 7:
        // "all expenses for user X", "all expenses in status Y",
        // "expenses between dates" — these speed up the report endpoints.
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ExpenseDate);
        builder.HasIndex(e => e.CategoryId);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}