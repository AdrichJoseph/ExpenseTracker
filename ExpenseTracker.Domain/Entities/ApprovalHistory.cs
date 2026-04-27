using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Entities;

// Append-only audit log. Every state transition writes one row here.
// Reports can query: "show me everything user X did to expense Y over time."
public class ApprovalHistory : Entity
{
    public required Guid ExpenseId { get; set; }
    public Expense? Expense { get; set; }

    // Who took the action (might be the submitter, or a manager, or admin).
    public required Guid ActorId { get; set; }
    public User? Actor { get; set; }

    public required ApprovalAction Action { get; set; }
    public string? Comment { get; set; }

    // Note: we don't need a separate Timestamp field — Entity.CreatedAt
    // already records when this audit row was created.
}