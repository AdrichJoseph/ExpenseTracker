namespace ExpenseTracker.Application.Reports.Dtos;

public record ReimbursementLagDto(
    double AverageDays,
    double MinDays,
    double MaxDays,
    int Count);
