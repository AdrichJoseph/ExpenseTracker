using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence;

// DbContext is EF Core's session-scoped class. One DbContext = one unit of work.
// In ASP.NET, we register it as Scoped (one per HTTP request).
public class AppDbContext : DbContext
{
    // This constructor receives DbContextOptions configured in Program.cs.
    // Options carry the connection string, provider choice (SQLite vs SQL Server), etc.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // ── DbSets ────────────────────────────────────────────────────────
    // Each DbSet<T> tells EF Core "T is a table". The property name becomes
    // the default table name (Users → "Users" table).

    public DbSet<User> Users => Set<User>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ApprovalHistory> ApprovalHistory => Set<ApprovalHistory>();
    public DbSet<Budget> Budgets => Set<Budget>();

    // ── Model configuration ───────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply ALL configurations from this assembly automatically.
        // Each entity's per-table config lives in Persistence/Configurations/*.cs
        // and implements IEntityTypeConfiguration<T>. This call discovers them.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    // ── Override SaveChanges to set timestamps automatically ──────────
    // Every time something is saved, this runs. We use it to enforce
    // soft-delete behavior at the persistence layer.
    public override int SaveChanges()
    {
        ApplySoftDeleteAndTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplySoftDeleteAndTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplySoftDeleteAndTimestamps()
    {
        // ChangeTracker knows which entities are being added/modified/deleted.
        var entries = ChangeTracker.Entries<Entity>();

        foreach (var entry in entries)
        {
            // If someone called Remove(), intercept it and mark soft-deleted instead.
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.SoftDelete();
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.MarkUpdated();
            }
            // For State.Added, the Entity base class already set CreatedAt = UtcNow
            // when the constructor ran, so no work needed.
        }
    }
}