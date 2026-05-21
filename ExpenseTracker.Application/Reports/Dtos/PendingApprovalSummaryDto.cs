namespace ExpenseTracker.Application.Reports.Dtos;

public record PendingApprovalSummaryDto(
    Guid? ManagerId,
    string ManagerName,
    int PendingCount,
    decimal TotalAmount);
