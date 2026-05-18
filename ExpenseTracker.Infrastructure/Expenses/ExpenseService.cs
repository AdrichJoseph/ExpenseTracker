using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Expenses;
using ExpenseTracker.Application.Expenses.Dtos;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Expenses;

public class ExpenseService : IExpenseService
{
    private readonly AppDbContext _db;

    public ExpenseService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ExpenseDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Expenses
            .AsNoTracking()
            .OrderByDescending(e => e.ExpenseDate)
            .Select(e => new ExpenseDto(
                e.Id,
                e.UserId,
                e.User!.Name,
                e.CategoryId,
                e.Category!.Name,
                e.Amount,
                e.Currency,
                e.Description,
                e.ExpenseDate,
                e.SubmittedDate,
                e.Status,
                e.ReceiptBlobUrl,
                e.ApproverId,
                e.ApprovedDate,
                e.RejectionReason,
                e.CreatedAt,
                e.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<Result<ExpenseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var expense = await _db.Expenses
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new ExpenseDto(
                e.Id, e.UserId, e.User!.Name, e.CategoryId, e.Category!.Name,
                e.Amount, e.Currency, e.Description, e.ExpenseDate,
                e.SubmittedDate, e.Status, e.ReceiptBlobUrl,
                e.ApproverId, e.ApprovedDate, e.RejectionReason,
                e.CreatedAt, e.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        return expense is null
            ? Result<ExpenseDto>.NotFound($"Expense {id} not found.")
            : Result<ExpenseDto>.Success(expense);
    }

    public async Task<Result<ExpenseDto>> CreateAsync(
        CreateExpenseRequest request,
        CancellationToken ct = default)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId, ct);
        if (!userExists)
            return Result<ExpenseDto>.Validation($"User {request.UserId} does not exist.");

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId, ct);
        if (!categoryExists)
            return Result<ExpenseDto>.Validation($"Category {request.CategoryId} does not exist.");

        var expense = new Expense
        {
            UserId = request.UserId,
            CategoryId = request.CategoryId,
            Amount = request.Amount,
            Currency = request.Currency,
            Description = request.Description,
            ExpenseDate = request.ExpenseDate,
        };

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(expense.Id, ct);
    }

    public async Task<Result<ExpenseDto>> UpdateAsync(
        Guid id,
        UpdateExpenseRequest request,
        CancellationToken ct = default)
    {
        var expense = await _db.Expenses.FindAsync(new object[] { id }, ct);

        if (expense is null)
            return Result<ExpenseDto>.NotFound($"Expense {id} not found.");

        if (expense.Status != Domain.Enums.ExpenseStatus.Draft)
            return Result<ExpenseDto>.Conflict(
                $"Cannot edit an expense in {expense.Status} status. Only drafts are editable.");

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId, ct);
        if (!categoryExists)
            return Result<ExpenseDto>.Validation($"Category {request.CategoryId} does not exist.");

        expense.CategoryId = request.CategoryId;
        expense.Amount = request.Amount;
        expense.Currency = request.Currency;
        expense.Description = request.Description;
        expense.ExpenseDate = request.ExpenseDate;

        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(expense.Id, ct);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var expense = await _db.Expenses.FindAsync(new object[] { id }, ct);

        if (expense is null)
            return Result<bool>.NotFound($"Expense {id} not found.");

        expense.SoftDelete();
        await _db.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
