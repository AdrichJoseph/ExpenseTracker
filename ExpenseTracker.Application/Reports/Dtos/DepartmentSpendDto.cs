namespace ExpenseTracker.Application.Reports.Dtos;

public record DepartmentSpendDto(
    string Department,
    decimal Total,
    int Count,
    decimal Average);
