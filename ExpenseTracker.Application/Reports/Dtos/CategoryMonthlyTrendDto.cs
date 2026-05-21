namespace ExpenseTracker.Application.Reports.Dtos;

public record CategoryMonthlyTrendDto(
    int Year,
    int Month,
    Guid CategoryId,
    string CategoryName,
    decimal Total,
    int Count);
