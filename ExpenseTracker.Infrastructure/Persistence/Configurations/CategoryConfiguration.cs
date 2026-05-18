using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Decimal column: 18 total digits, 2 after the decimal point.
        // Money up to 9 999 999 999 999 999.99 — safely overkill.
        builder.Property(c => c.MonthlyLimit)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(c => c.Name).IsUnique();

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}