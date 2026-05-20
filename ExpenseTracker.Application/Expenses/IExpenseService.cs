using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Expenses.Dtos;

namespace ExpenseTracker.Application.Expenses;

public interface IExpenseService
{
    Task<IReadOnlyList<ExpenseDto>> GetAllAsync(CurrentUser caller, CancellationToken ct = default);
    Task<Result<ExpenseDto>> GetByIdAsync(Guid id, CurrentUser caller, CancellationToken ct = default);
    Task<Result<ExpenseDto>> CreateAsync(CreateExpenseRequest request, CurrentUser caller, CancellationToken ct = default);
    Task<Result<ExpenseDto>> UpdateAsync(Guid id, UpdateExpenseRequest request, CurrentUser caller, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(Guid id, CurrentUser caller, CancellationToken ct = default);
}