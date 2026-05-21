using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Expenses.Dtos;

public record ExpenseFilterRequest(
    int Page = 1,
    int PageSize = 20,
    ExpenseStatus? Status = null,
    DateTime? From = null,
    DateTime? To = null,
    Guid? CategoryId = null,
    Guid? UserId = null);
