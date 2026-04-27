using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class Category : Entity
{
    public required string Name { get; set; }

    // decimal, NOT double or float — money math with binary floats is wrong.
    // 0.1 + 0.2 != 0.3 in float, but it equals 0.3 in decimal.
    // Use decimal for ALL currency in C#.
    public decimal? MonthlyLimit { get; set; }

    // Reverse navigation
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}