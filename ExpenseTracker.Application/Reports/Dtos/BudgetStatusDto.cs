namespace ExpenseTracker.Application.Reports.Dtos;

public record BudgetStatusDto(
    string Department,
    Guid CategoryId,
    string CategoryName,
    decimal Budget,
    decimal Actual,
    decimal Variance,      // Budget − Actual; negative means over budget
    bool IsOverBudget);
