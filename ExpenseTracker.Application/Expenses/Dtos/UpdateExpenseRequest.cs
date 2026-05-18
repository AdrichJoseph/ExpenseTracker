namespace ExpenseTracker.Application.Expenses.Dtos;

// What PUT /api/expenses/{id} accepts. Only fields the user can change while a draft.
// Status changes happen through dedicated workflow endpoints in Phase 7.
public record UpdateExpenseRequest(
    Guid CategoryId,
    decimal Amount,
    string Currency,
    string Description,
    DateTime ExpenseDate);