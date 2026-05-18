using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Expenses.Dtos;

namespace ExpenseTracker.Application.Expenses;

public interface IExpenseService
{
    Task<IReadOnlyList<ExpenseDto>> GetAllAsync(CancellationToken ct = default);
    Task<Result<ExpenseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<ExpenseDto>> CreateAsync(CreateExpenseRequest request, CancellationToken ct = default);
    Task<Result<ExpenseDto>> UpdateAsync(Guid id, UpdateExpenseRequest request, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken ct = default);
}
