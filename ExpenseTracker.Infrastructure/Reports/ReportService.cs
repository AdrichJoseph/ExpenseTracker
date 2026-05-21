using ExpenseTracker.Application.Reports;
using ExpenseTracker.Application.Reports.Dtos;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Reports;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db) => _db = db;

    // ── 1. BY CATEGORY ────────────────────────────────────────────────────────
    public async Task<IReadOnlyList<CategorySpendDto>> GetByCategoryAsync(
        DateTime? dateFrom, DateTime? dateTo, CancellationToken ct = default)
    {
        var q =
            from e in _db.Expenses.AsNoTracking()
            join c in _db.Categories.AsNoTracking() on e.CategoryId equals c.Id
            where e.Status != ExpenseStatus.Draft
               && (!dateFrom.HasValue || e.ExpenseDate >= dateFrom.Value)
               && (!dateTo.HasValue   || e.ExpenseDate <= dateTo.Value)
            group new { e, c } by new { e.CategoryId, c.Name } into g
            orderby g.Sum(x => x.e.Amount) descending
            select new CategorySpendDto(
                g.Key.CategoryId,
                g.Key.Name,
                g.Sum(x => x.e.Amount),
                g.Count(),
                g.Average(x => x.e.Amount));

        return await q.ToListAsync(ct);
    }

    // ── 2. BY DEPARTMENT ──────────────────────────────────────────────────────
    public async Task<IReadOnlyList<DepartmentSpendDto>> GetByDepartmentAsync(
        DateTime? dateFrom, DateTime? dateTo, CancellationToken ct = default)
    {
        var q =
            from e in _db.Expenses.AsNoTracking()
            join u in _db.Users.AsNoTracking() on e.UserId equals u.Id
            where e.Status != ExpenseStatus.Draft
               && u.Department != null
               && (!dateFrom.HasValue || e.ExpenseDate >= dateFrom.Value)
               && (!dateTo.HasValue   || e.ExpenseDate <= dateTo.Value)
            group e by u.Department into g
            orderby g.Sum(e => e.Amount) descending
            select new DepartmentSpendDto(
                g.Key!,
                g.Sum(e => e.Amount),
                g.Count(),
                g.Average(e => e.Amount));

        return await q.ToListAsync(ct);
    }

    // ── 3. TOP SPENDERS ───────────────────────────────────────────────────────
    public async Task<IReadOnlyList<TopSpenderDto>> GetTopSpendersAsync(
        int topN, DateTime? dateFrom, DateTime? dateTo, CancellationToken ct = default)
    {
        var q =
            from e in _db.Expenses.AsNoTracking()
            join u in _db.Users.AsNoTracking() on e.UserId equals u.Id
            where e.Status != ExpenseStatus.Draft
               && (!dateFrom.HasValue || e.ExpenseDate >= dateFrom.Value)
               && (!dateTo.HasValue   || e.ExpenseDate <= dateTo.Value)
            group new { e, u } by new { e.UserId, u.Name, u.Department } into g
            orderby g.Sum(x => x.e.Amount) descending
            select new TopSpenderDto(
                g.Key.UserId,
                g.Key.Name,
                g.Key.Department ?? string.Empty,
                g.Sum(x => x.e.Amount),
                g.Count());

        return await q.Take(topN).ToListAsync(ct);
    }

    // ── 4. ALL USERS (no cut-off) ─────────────────────────────────────────────
    public async Task<IReadOnlyList<TopSpenderDto>> GetByUserAsync(
        DateTime? dateFrom, DateTime? dateTo, CancellationToken ct = default)
    {
        var q =
            from e in _db.Expenses.AsNoTracking()
            join u in _db.Users.AsNoTracking() on e.UserId equals u.Id
            where e.Status != ExpenseStatus.Draft
               && (!dateFrom.HasValue || e.ExpenseDate >= dateFrom.Value)
               && (!dateTo.HasValue   || e.ExpenseDate <= dateTo.Value)
            group new { e, u } by new { e.UserId, u.Name, u.Department } into g
            orderby g.Key.Name
            select new TopSpenderDto(
                g.Key.UserId,
                g.Key.Name,
                g.Key.Department ?? string.Empty,
                g.Sum(x => x.e.Amount),
                g.Count());

        return await q.ToListAsync(ct);
    }

    // ── 5. MONTHLY TREND ──────────────────────────────────────────────────────
    public async Task<IReadOnlyList<MonthlyTrendDto>> GetMonthlyTrendAsync(
        int months, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddMonths(-months);

        var q =
            from e in _db.Expenses.AsNoTracking()
            where e.Status != ExpenseStatus.Draft && e.ExpenseDate >= cutoff
            group e by new { e.ExpenseDate.Year, e.ExpenseDate.Month } into g
            orderby g.Key.Year, g.Key.Month
            select new MonthlyTrendDto(
                g.Key.Year,
                g.Key.Month,
                g.Sum(e => e.Amount),
                g.Count());

        return await q.ToListAsync(ct);
    }

    // ── 6. DAILY TREND ────────────────────────────────────────────────────────
    // Groups by Year/Month/Day ints rather than DateTime.Date — more portable
    // across EF/SQLite translation; date is reconstructed after ToListAsync.
    public async Task<IReadOnlyList<DailyTrendDto>> GetDailyTrendAsync(
        int days, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.Date.AddDays(-days + 1);

        var raw = await (
            from e in _db.Expenses.AsNoTracking()
            where e.Status != ExpenseStatus.Draft && e.ExpenseDate >= cutoff
            group e by new { e.ExpenseDate.Year, e.ExpenseDate.Month, e.ExpenseDate.Day } into g
            orderby g.Key.Year, g.Key.Month, g.Key.Day
            select new
            {
                g.Key.Year, g.Key.Month, g.Key.Day,
                Total = g.Sum(e => e.Amount),
                Count = g.Count()
            }
        ).ToListAsync(ct);

        return raw
            .Select(r => new DailyTrendDto(new DateTime(r.Year, r.Month, r.Day), r.Total, r.Count))
            .ToList();
    }

    // ── 7. CATEGORY × MONTH CROSS-TAB ────────────────────────────────────────
    public async Task<IReadOnlyList<CategoryMonthlyTrendDto>> GetCategoryTrendAsync(
        int months, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddMonths(-months);

        var q =
            from e in _db.Expenses.AsNoTracking()
            join c in _db.Categories.AsNoTracking() on e.CategoryId equals c.Id
            where e.Status != ExpenseStatus.Draft && e.ExpenseDate >= cutoff
            group new { e, c } by new { e.ExpenseDate.Year, e.ExpenseDate.Month, e.CategoryId, c.Name } into g
            orderby g.Key.Year, g.Key.Month, g.Key.Name
            select new CategoryMonthlyTrendDto(
                g.Key.Year,
                g.Key.Month,
                g.Key.CategoryId,
                g.Key.Name,
                g.Sum(x => x.e.Amount),
                g.Count());

        return await q.ToListAsync(ct);
    }

    // ── 8. BY STATUS ──────────────────────────────────────────────────────────
    public async Task<IReadOnlyList<StatusSummaryDto>> GetByStatusAsync(
        CancellationToken ct = default)
    {
        var q =
            from e in _db.Expenses.AsNoTracking()
            group e by e.Status into g
            orderby (int)g.Key
            select new StatusSummaryDto(
                g.Key,
                g.Key.ToString(),
                g.Count(),
                g.Sum(e => e.Amount));

        return await q.ToListAsync(ct);
    }

    // ── 9. BUDGET STATUS (all rows) ───────────────────────────────────────────
    // Two queries + in-memory join: a single SQL left-join of an aggregate
    // subquery against Budgets translates poorly in EF/SQLite.
    public async Task<IReadOnlyList<BudgetStatusDto>> GetBudgetStatusAsync(
        DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
    {
        var budgets = await _db.Budgets.AsNoTracking()
            .Include(b => b.Category)
            .Where(b => b.PeriodStart <= periodEnd && b.PeriodEnd >= periodStart)
            .ToListAsync(ct);

        var actuals = await (
            from e in _db.Expenses.AsNoTracking()
            join u in _db.Users.AsNoTracking() on e.UserId equals u.Id
            where e.Status != ExpenseStatus.Draft
               && e.ExpenseDate >= periodStart
               && e.ExpenseDate <= periodEnd
               && u.Department != null
            group e by new { e.CategoryId, u.Department } into g
            select new { g.Key.CategoryId, Department = g.Key.Department!, Actual = g.Sum(e => e.Amount) }
        ).ToListAsync(ct);

        return budgets.Select(b =>
        {
            var actual = actuals
                .FirstOrDefault(a => a.CategoryId == b.CategoryId && a.Department == b.Department)
                ?.Actual ?? 0m;
            var variance = b.Amount - actual;
            return new BudgetStatusDto(b.Department, b.CategoryId, b.Category!.Name,
                b.Amount, actual, variance, actual > b.Amount);
        }).ToList();
    }

    // ── 10. OVER BUDGET ───────────────────────────────────────────────────────
    public async Task<IReadOnlyList<BudgetStatusDto>> GetOverBudgetAsync(
        DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
    {
        var all = await GetBudgetStatusAsync(periodStart, periodEnd, ct);
        return all.Where(b => b.IsOverBudget).ToList();
    }

    // ── 11. PENDING APPROVAL SUMMARY ─────────────────────────────────────────
    // Left-joins Users → Manager to group orphan expenses (no manager) under
    // a "No Manager" bucket rather than dropping them.
    public async Task<IReadOnlyList<PendingApprovalSummaryDto>> GetPendingApprovalSummaryAsync(
        CancellationToken ct = default)
    {
        return await (
            from e in _db.Expenses.AsNoTracking()
            join u in _db.Users.AsNoTracking() on e.UserId equals u.Id
            join m in _db.Users.AsNoTracking() on u.ManagerId equals m.Id into mgrLeft
            from m in mgrLeft.DefaultIfEmpty()
            where e.Status == ExpenseStatus.Submitted
            group new { e, m } by new
            {
                ManagerId   = m == null ? (Guid?)null : (Guid?)m.Id,
                ManagerName = m == null ? "No Manager" : m.Name
            } into g
            orderby g.Sum(x => x.e.Amount) descending
            select new PendingApprovalSummaryDto(
                g.Key.ManagerId,
                g.Key.ManagerName,
                g.Count(),
                g.Sum(x => x.e.Amount))
        ).ToListAsync(ct);
    }

    // ── 12. REIMBURSEMENT LAG ─────────────────────────────────────────────────
    // Projects only the two date columns before pulling to memory; EF can't
    // translate (DateTime - DateTime).TotalDays to SQL across all providers.
    public async Task<ReimbursementLagDto> GetReimbursementLagAsync(
        DateTime? dateFrom, DateTime? dateTo, CancellationToken ct = default)
    {
        var pairs = await _db.Expenses
            .AsNoTracking()
            .Where(e => (e.Status == ExpenseStatus.Approved || e.Status == ExpenseStatus.Reimbursed)
                     && e.SubmittedDate.HasValue
                     && e.ApprovedDate.HasValue
                     && (!dateFrom.HasValue || e.SubmittedDate >= dateFrom.Value)
                     && (!dateTo.HasValue   || e.SubmittedDate <= dateTo.Value))
            .Select(e => new { e.SubmittedDate, e.ApprovedDate })
            .ToListAsync(ct);

        if (pairs.Count == 0)
            return new ReimbursementLagDto(0, 0, 0, 0);

        var lags = pairs
            .Select(p => (p.ApprovedDate!.Value - p.SubmittedDate!.Value).TotalDays)
            .ToList();

        return new ReimbursementLagDto(lags.Average(), lags.Min(), lags.Max(), lags.Count);
    }
}
