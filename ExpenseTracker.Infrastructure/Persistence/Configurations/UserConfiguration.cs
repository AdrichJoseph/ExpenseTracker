using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Persistence.Configurations;

// IEntityTypeConfiguration<T> is the EF Core convention for per-entity config.
// AppDbContext.OnModelCreating discovers all of these via reflection.
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        // String column constraints — without these, EF defaults to nvarchar(MAX),
        // which is wasteful and prevents indexing. Explicit lengths = better SQL.
        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(320);  // RFC 5321 max email length

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.Department)
            .HasMaxLength(100);

        // Store enum as int (the default) — the explicit numbering in our enum
        // (Employee=1, Manager=2, Admin=3) means the DB values are stable.
        builder.Property(u => u.Role)
            .HasConversion<int>();

        // Self-referential FK: a user has a manager who is a user.
        builder.HasOne(u => u.Manager)
            .WithMany()                 // we don't expose "Reports" navigation, keep it simple
            .HasForeignKey(u => u.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);  // can't delete a manager who has reports

        // One user → many expenses
        builder.HasMany(u => u.Expenses)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes — make Email unique (no duplicate accounts) and faster to look up.
        builder.HasIndex(u => u.Email).IsUnique();

        // Soft-delete global filter: any LINQ query that includes Users
        // automatically appends WHERE IsDeleted = 0. Magic.
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}