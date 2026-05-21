namespace ExpenseTracker.Application.Reports.Dtos;

public record MonthlyTrendDto(
    int Year,
    int Month,
    decimal Total,
    int Count);
