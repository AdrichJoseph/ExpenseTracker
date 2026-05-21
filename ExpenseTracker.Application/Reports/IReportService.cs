using ExpenseTracker.Application.Reports.Dtos;

namespace ExpenseTracker.Application.Reports;

public interface IReportService
{
    // Total + count + average spend grouped by category (optional date window).
    Task<IReadOnlyList<CategorySpendDto>> GetByCategoryAsync(DateTime? dateFrom, DateTime? dateTo, CancellationToken ct = default);

    // Total + count + average spend grouped by department.
    Task<IReadOnlyList<DepartmentSpendDto>> GetByDepartmentAsync(DateTime? dateFrom, DateTime? dateTo, CancellationToken ct = default);

    // Top N users by total spend, descending.
    Task<IReadOnlyList<TopSpenderDto>> GetTopSpendersAsync(int topN, DateTime? dateFrom, DateTime? dateTo, CancellationToken ct = default);

    // Every user's spend totals (no cut-off), used by HR / finance.
    Task<IReadOnlyList<TopSpenderDto>> GetByUserAsync(DateTime? dateFrom, DateTime? dateTo, CancellationToken ct = default);

    // Monthly spend totals over the last N calendar months.
    Task<IReadOnlyList<MonthlyTrendDto>> GetMonthlyTrendAsync(int months, CancellationToken ct = default);

    // Daily spend totals over the last N days.
    Task<IReadOnlyList<DailyTrendDto>> GetDailyTrendAsync(int days, CancellationToken ct = default);

    // Per-category spend per month — the cross-tab for charting.
    Task<IReadOnlyList<CategoryMonthlyTrendDto>> GetCategoryTrendAsync(int months, CancellationToken ct = default);

    // Expense count + total per status across the whole system.
    Task<IReadOnlyList<StatusSummaryDto>> GetByStatusAsync(CancellationToken ct = default);

    // Budget vs actual for a given period; includes under-budget rows too.
    Task<IReadOnlyList<BudgetStatusDto>> GetBudgetStatusAsync(DateTime periodStart, DateTime periodEnd, CancellationToken ct = default);

    // Only the rows where actual > budget.
    Task<IReadOnlyList<BudgetStatusDto>> GetOverBudgetAsync(DateTime periodStart, DateTime periodEnd, CancellationToken ct = default);

    // Per-manager: how many submitted expenses are waiting for action.
    Task<IReadOnlyList<PendingApprovalSummaryDto>> GetPendingApprovalSummaryAsync(CancellationToken ct = default);

    // Average / min / max days from Submitted → Approved.
    Task<ReimbursementLagDto> GetReimbursementLagAsync(DateTime? dateFrom, DateTime? dateTo, CancellationToken ct = default);
}
