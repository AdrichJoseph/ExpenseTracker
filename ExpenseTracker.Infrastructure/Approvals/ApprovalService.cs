using ExpenseTracker.Application.Approvals;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Expenses;
using ExpenseTracker.Application.Expenses.Dtos;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Approvals;

public class ApprovalService : IApprovalService
{
    private readonly AppDbContext _db;
    private readonly IExpenseService _expenses;

    public ApprovalService(AppDbContext db, IExpenseService expenses)
    {
        _db = db;
        _expenses = expenses;
    }

    // Same role-based scope as ExpenseService.VisibleExpenses.
    // Duplicated intentionally — sharing it would require extracting a query helper
    // class that has no other purpose right now.
    private IQueryable<Expense> VisibleExpenses(CurrentUser caller)
    {
        var query = _db.Expenses.AsNoTracking();

        if (caller.Role == Role.Admin)
            return query;

        if (caller.Role == Role.Manager)
        {
            var reportIds = _db.Users
                .Where(u => u.ManagerId == caller.Id)
                .Select(u => u.Id);
            return query.Where(e => e.UserId == caller.Id || reportIds.Contains(e.UserId));
        }

        return query.Where(e => e.UserId == caller.Id);
    }

    public async Task<Result<ExpenseDto>> SubmitAsync(
        Guid expenseId, CurrentUser caller, CancellationToken ct = default)
    {
        // Employees may only submit their own expense; Managers/Admins can submit any visible one.
        var accessible = caller.Role == Role.Employee
            ? await _db.Expenses.AsNoTracking()
                .AnyAsync(e => e.Id == expenseId && e.UserId == caller.Id, ct)
            : await VisibleExpenses(caller).AnyAsync(e => e.Id == expenseId, ct);

        if (!accessible)
            return Result<ExpenseDto>.NotFound($"Expense {expenseId} not found or not accessible.");

        var expense = await _db.Expenses
            .Include(e => e.ApprovalHistory)
            .FirstAsync(e => e.Id == expenseId, ct);

        expense.Submit();

        await _db.SaveChangesAsync(ct);
        return await _expenses.GetByIdAsync(expenseId, caller, ct);
    }

    public async Task<Result<ExpenseDto>> ApproveAsync(
        Guid expenseId, string? comment, CurrentUser caller, CancellationToken ct = default)
    {
        if (caller.Role == Role.Employee)
            return Result<ExpenseDto>.Conflict("Employees cannot approve expenses.");

        var visible = await VisibleExpenses(caller).AnyAsync(e => e.Id == expenseId, ct);
        if (!visible)
            return Result<ExpenseDto>.NotFound($"Expense {expenseId} not found or not accessible.");

        var expense = await _db.Expenses
            .Include(e => e.ApprovalHistory)
            .FirstAsync(e => e.Id == expenseId, ct);

        expense.Approve(caller.Id, comment);

        await _db.SaveChangesAsync(ct);
        return await _expenses.GetByIdAsync(expenseId, caller, ct);
    }

    public async Task<Result<ExpenseDto>> RejectAsync(
        Guid expenseId, string reason, CurrentUser caller, CancellationToken ct = default)
    {
        if (caller.Role == Role.Employee)
            return Result<ExpenseDto>.Conflict("Employees cannot reject expenses.");

        if (string.IsNullOrWhiteSpace(reason))
            return Result<ExpenseDto>.Validation("A rejection reason is required.");

        var visible = await VisibleExpenses(caller).AnyAsync(e => e.Id == expenseId, ct);
        if (!visible)
            return Result<ExpenseDto>.NotFound($"Expense {expenseId} not found or not accessible.");

        var expense = await _db.Expenses
            .Include(e => e.ApprovalHistory)
            .FirstAsync(e => e.Id == expenseId, ct);

        expense.Reject(caller.Id, reason);

        await _db.SaveChangesAsync(ct);
        return await _expenses.GetByIdAsync(expenseId, caller, ct);
    }

    public async Task<IReadOnlyList<ExpenseDto>> GetPendingAsync(
        CurrentUser caller, CancellationToken ct = default)
    {
        // Oldest first — the expense that has been waiting longest is most urgent.
        var query =
            from e in VisibleExpenses(caller)
            join u in _db.Users.AsNoTracking() on e.UserId equals u.Id
            join c in _db.Categories.AsNoTracking() on e.CategoryId equals c.Id
            where e.Status == ExpenseStatus.Submitted
            orderby e.SubmittedDate ascending
            select new ExpenseDto(
                e.Id, e.UserId, u.Name, e.CategoryId, c.Name,
                e.Amount, e.Currency, e.Description, e.ExpenseDate,
                e.SubmittedDate, e.Status, e.ReceiptBlobUrl,
                e.ApproverId, e.ApprovedDate, e.RejectionReason,
                e.CreatedAt, e.UpdatedAt);

        return await query.ToListAsync(ct);
    }
}
