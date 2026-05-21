namespace ExpenseTracker.Application.Reports.Dtos;

public record DailyTrendDto(
    DateTime Date,
    decimal Total,
    int Count);
