using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Entities;

// Append-only audit log. Every state transition writes one row here.
// Like Expense, this entity keeps the ActorId Guid but no navigation property
// to the actual User (which lives in Infrastructure).
public class ApprovalHistory : Entity
{
    public required Guid ExpenseId { get; set; }
    public Expense? Expense { get; set; }

    // Who took the action — Guid only; the C# navigation lives in Infrastructure.
    public required Guid ActorId { get; set; }

    public required ApprovalAction Action { get; set; }
    public string? Comment { get; set; }
}
