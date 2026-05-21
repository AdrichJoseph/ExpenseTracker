using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Expenses;
using ExpenseTracker.Application.Expenses.Dtos;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Infrastructure.Identity;
using ExpenseTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Expenses;

public class ExpenseService : IExpenseService
{
    private readonly AppDbContext _db;

    public ExpenseService(AppDbContext db) => _db = db;

    // Builds the base expense query, scoped to what the caller is allowed to see.
    // Admin: everything. Manager: own + direct reports'. Employee: own only.
    private IQueryable<Expense> VisibleExpenses(CurrentUser caller)
    {
        var query = _db.Expenses.AsNoTracking();

        if (caller.Role == Role.Admin)
            return query;

        if (caller.Role == Role.Manager)
        {
            // Own expenses, plus expenses of users who report to this manager.
            var reportIds = _db.Users
                .Where(u => u.ManagerId == caller.Id)
                .Select(u => u.Id);

            return query.Where(e => e.UserId == caller.Id || reportIds.Contains(e.UserId));
        }

        // Employee: only their own.
        return query.Where(e => e.UserId == caller.Id);
    }

    public async Task<PagedResult<ExpenseDto>> GetPagedAsync(
        ExpenseFilterRequest filter, CurrentUser caller, CancellationToken ct = default)
    {
        // Build the base join first, then layer conditional Where clauses on top.
        // Nothing hits the database until CountAsync / ToListAsync.
        var baseQuery =
            from e in VisibleExpenses(caller)
            join u in _db.Users.AsNoTracking() on e.UserId equals u.Id
            join c in _db.Categories.AsNoTracking() on e.CategoryId equals c.Id
            select new { e, u, c };

        if (filter.Status.HasValue)
            baseQuery = baseQuery.Where(x => x.e.Status == filter.Status.Value);

        if (filter.From.HasValue)
            baseQuery = baseQuery.Where(x => x.e.ExpenseDate >= filter.From.Value);

        if (filter.To.HasValue)
            baseQuery = baseQuery.Where(x => x.e.ExpenseDate <= filter.To.Value);

        if (filter.CategoryId.HasValue)
            baseQuery = baseQuery.Where(x => x.e.CategoryId == filter.CategoryId.Value);

        if (filter.UserId.HasValue)
            baseQuery = baseQuery.Where(x => x.e.UserId == filter.UserId.Value);

        var totalCount = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .OrderByDescending(x => x.e.ExpenseDate)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new ExpenseDto(
                x.e.Id, x.e.UserId, x.u.Name, x.e.CategoryId, x.c.Name,
                x.e.Amount, x.e.Currency, x.e.Description, x.e.ExpenseDate,
                x.e.SubmittedDate, x.e.Status, x.e.ReceiptBlobUrl,
                x.e.ApproverId, x.e.ApprovedDate, x.e.RejectionReason,
                x.e.CreatedAt, x.e.UpdatedAt))
            .ToListAsync(ct);

        return new PagedResult<ExpenseDto>(items, filter.Page, filter.PageSize, totalCount);
    }

    public async Task<Result<ExpenseDto>> GetByIdAsync(Guid id, CurrentUser caller, CancellationToken ct = default)
    {
        var query =
            from e in VisibleExpenses(caller)
            join u in _db.Users.AsNoTracking() on e.UserId equals u.Id
            join c in _db.Categories.AsNoTracking() on e.CategoryId equals c.Id
            where e.Id == id
            select new ExpenseDto(
                e.Id, e.UserId, u.Name, e.CategoryId, c.Name,
                e.Amount, e.Currency, e.Description, e.ExpenseDate,
                e.SubmittedDate, e.Status, e.ReceiptBlobUrl,
                e.ApproverId, e.ApprovedDate, e.RejectionReason,
                e.CreatedAt, e.UpdatedAt);

        var expense = await query.FirstOrDefaultAsync(ct);

        return expense is null
            ? Result<ExpenseDto>.NotFound($"Expense {id} not found or not accessible.")
            : Result<ExpenseDto>.Success(expense);
    }

    public async Task<Result<ExpenseDto>> CreateAsync(
        CreateExpenseRequest request, CurrentUser caller, CancellationToken ct = default)
    {
        // An employee can only create expenses for themselves.
        // Admins/Managers may create on behalf of others.
        var targetUserId = caller.Role == Role.Employee ? caller.Id : request.UserId;

        var userExists = await _db.Users.AnyAsync(u => u.Id == targetUserId, ct);
        if (!userExists)
            return Result<ExpenseDto>.Validation($"User {targetUserId} does not exist.");

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId, ct);
        if (!categoryExists)
            return Result<ExpenseDto>.Validation($"Category {request.CategoryId} does not exist.");

        var expense = new Expense
        {
            UserId = targetUserId,
            CategoryId = request.CategoryId,
            Amount = request.Amount,
            Currency = request.Currency,
            Description = request.Description,
            ExpenseDate = request.ExpenseDate,
        };

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(expense.Id, caller, ct);
    }

    public async Task<Result<ExpenseDto>> UpdateAsync(
        Guid id, UpdateExpenseRequest request, CurrentUser caller, CancellationToken ct = default)
    {
        // Find it within what the caller can see — prevents editing others' expenses.
        var expense = await VisibleExpenses(caller).FirstOrDefaultAsync(e => e.Id == id, ct);
        if (expense is null)
            return Result<ExpenseDto>.NotFound($"Expense {id} not found or not accessible.");

        if (expense.Status != ExpenseStatus.Draft)
            return Result<ExpenseDto>.Conflict(
                $"Cannot edit an expense in {expense.Status} status. Only drafts are editable.");

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId, ct);
        if (!categoryExists)
            return Result<ExpenseDto>.Validation($"Category {request.CategoryId} does not exist.");

        // Re-fetch as tracked so EF saves the changes.
        var tracked = await _db.Expenses.FirstAsync(e => e.Id == id, ct);
        tracked.CategoryId = request.CategoryId;
        tracked.Amount = request.Amount;
        tracked.Currency = request.Currency;
        tracked.Description = request.Description;
        tracked.ExpenseDate = request.ExpenseDate;

        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(id, caller, ct);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CurrentUser caller, CancellationToken ct = default)
    {
        var visible = await VisibleExpenses(caller).AnyAsync(e => e.Id == id, ct);
        if (!visible)
            return Result<bool>.NotFound($"Expense {id} not found or not accessible.");

        var tracked = await _db.Expenses.FirstAsync(e => e.Id == id, ct);
        tracked.SoftDelete();
        await _db.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}