using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Expenses.Dtos;

// What GET /api/expenses/{id} returns. A flat projection of the entity
// designed for API consumers — no navigation properties, no internal state.
public record ExpenseDto(
    Guid Id,
    Guid UserId,
    string UserName,
    Guid CategoryId,
    string CategoryName,
    decimal Amount,
    string Currency,
    string Description,
    DateTime ExpenseDate,
    DateTime? SubmittedDate,
    ExpenseStatus Status,
    string? ReceiptBlobUrl,
    Guid? ApproverId,
    DateTime? ApprovedDate,
    string? RejectionReason,
    DateTime CreatedAt,
    DateTime? UpdatedAt);