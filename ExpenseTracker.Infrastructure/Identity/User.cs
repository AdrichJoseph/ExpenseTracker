using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Infrastructure.Identity;

// Lives in Infrastructure because it depends on ASP.NET Identity.
// Domain stays framework-agnostic. If we ever swap identity providers,
// only this file (and the AppDbContext registration) changes.
public class User : IdentityUser<Guid>
{
    public required string Name { get; set; }
    public required Role Role { get; set; }
    public string? Department { get; set; }

    public Guid? ManagerId { get; set; }
    public User? Manager { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation back to expenses owned by this user
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
