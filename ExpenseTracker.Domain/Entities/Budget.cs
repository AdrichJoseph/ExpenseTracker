using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class Budget : Entity
{
    // Department names are strings — could be an enum, but real-world
    // departments change too often to hard-code. Keeping it flexible.
    public required string Department { get; set; }

    public required Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public decimal Amount { get; set; }

    // First day of the period; period length determined by the report.
    // For now: monthly. (We could add a Period enum later if needed.)
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}