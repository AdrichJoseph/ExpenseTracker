using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Reports.Dtos;

public record StatusSummaryDto(
    ExpenseStatus Status,
    string StatusName,
    int Count,
    decimal Total);
