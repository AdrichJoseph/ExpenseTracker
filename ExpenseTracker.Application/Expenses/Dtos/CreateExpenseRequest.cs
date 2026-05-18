namespace ExpenseTracker.Application.Expenses.Dtos;

// What POST /api/expenses accepts. Notice what's MISSING:
//  - No Id (server generates it)
//  - No Status (always starts as Draft)
//  - No ApproverId, RejectionReason, etc. (those come from workflow methods, not the client)
public record CreateExpenseRequest(
    Guid UserId,
    Guid CategoryId,
    decimal Amount,
    string Currency,
    string Description,
    DateTime ExpenseDate);