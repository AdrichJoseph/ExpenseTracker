using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Entities;

public class User : Entity
{
    // `required` (C# 11+) means: you MUST set this when constructing a User.
    // The compiler refuses to let you `new User()` without giving these.
    // It replaces the old "force constructor parameters" pattern with cleaner syntax.
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required Role Role { get; set; }
    public string? Department { get; set; }

    // Self-referential foreign key: a User has a Manager, who is also a User.
    // Nullable because Admins / top-of-org-chart don't have managers.
    public Guid? ManagerId { get; set; }
    public User? Manager { get; set; }

    // We're storing a hashed password here for now (Phase 6 will swap to ASP.NET Identity).
    // NEVER store plaintext passwords. EVER.
    public string PasswordHash { get; set; } = string.Empty;

    // Navigation property: an expense has a UserId pointing to this user.
    // EF Core will populate this collection when we Include() it in queries.
    // ICollection<T> is the "any collection" interface — we use it because EF Core
    // expects it specifically (List<T> works too but ICollection is the convention).
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}