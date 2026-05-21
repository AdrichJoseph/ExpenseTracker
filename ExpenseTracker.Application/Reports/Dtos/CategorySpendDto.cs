namespace ExpenseTracker.Application.Reports.Dtos;

public record CategorySpendDto(
    Guid CategoryId,
    string CategoryName,
    decimal Total,
    int Count,
    decimal Average);
