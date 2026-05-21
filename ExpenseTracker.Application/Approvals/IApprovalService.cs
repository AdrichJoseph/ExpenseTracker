using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Expenses.Dtos;

namespace ExpenseTracker.Application.Approvals;

public interface IApprovalService
{
    // Draft → Submitted. Caller must own the expense (or be Admin/Manager).
    Task<Result<ExpenseDto>> SubmitAsync(Guid expenseId, CurrentUser caller, CancellationToken ct = default);

    // Submitted → Approved. Manager/Admin only; domain blocks self-approval.
    Task<Result<ExpenseDto>> ApproveAsync(Guid expenseId, string? comment, CurrentUser caller, CancellationToken ct = default);

    // Submitted → Rejected. Manager/Admin only; reason required.
    Task<Result<ExpenseDto>> RejectAsync(Guid expenseId, string reason, CurrentUser caller, CancellationToken ct = default);

    // All Submitted expenses visible to the caller — their work queue.
    Task<IReadOnlyList<ExpenseDto>> GetPendingAsync(CurrentUser caller, CancellationToken ct = default);
}
