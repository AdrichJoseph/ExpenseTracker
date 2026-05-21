using ExpenseTracker.Application.Reports;
using ExpenseTracker.Application.Reports.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service) => _service = service;

    /// <summary>Total/count/average spend per category. ?from= &amp;to= are optional ISO dates.</summary>
    [HttpGet("by-category")]
    public async Task<ActionResult<IReadOnlyList<CategorySpendDto>>> ByCategory(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await _service.GetByCategoryAsync(from, to, ct));

    /// <summary>Total/count/average spend per department.</summary>
    [HttpGet("by-department")]
    public async Task<ActionResult<IReadOnlyList<DepartmentSpendDto>>> ByDepartment(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await _service.GetByDepartmentAsync(from, to, ct));

    /// <summary>Top N users by spend. ?topN= defaults to 10.</summary>
    [HttpGet("top-spenders")]
    public async Task<ActionResult<IReadOnlyList<TopSpenderDto>>> TopSpenders(
        [FromQuery] int topN = 10,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
        => Ok(await _service.GetTopSpendersAsync(topN, from, to, ct));

    /// <summary>Every user's spend totals (no cut-off).</summary>
    [HttpGet("by-user")]
    public async Task<ActionResult<IReadOnlyList<TopSpenderDto>>> ByUser(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await _service.GetByUserAsync(from, to, ct));

    /// <summary>Monthly spend totals over the last N calendar months. ?months= defaults to 12.</summary>
    [HttpGet("monthly-trend")]
    public async Task<ActionResult<IReadOnlyList<MonthlyTrendDto>>> MonthlyTrend(
        [FromQuery] int months = 12, CancellationToken ct = default)
        => Ok(await _service.GetMonthlyTrendAsync(months, ct));

    /// <summary>Daily spend totals over the last N days. ?days= defaults to 30.</summary>
    [HttpGet("daily-trend")]
    public async Task<ActionResult<IReadOnlyList<DailyTrendDto>>> DailyTrend(
        [FromQuery] int days = 30, CancellationToken ct = default)
        => Ok(await _service.GetDailyTrendAsync(days, ct));

    /// <summary>Per-category spend per month — cross-tab data for charts.</summary>
    [HttpGet("category-trend")]
    public async Task<ActionResult<IReadOnlyList<CategoryMonthlyTrendDto>>> CategoryTrend(
        [FromQuery] int months = 6, CancellationToken ct = default)
        => Ok(await _service.GetCategoryTrendAsync(months, ct));

    /// <summary>Expense count + total per status across the whole system.</summary>
    [HttpGet("by-status")]
    public async Task<ActionResult<IReadOnlyList<StatusSummaryDto>>> ByStatus(CancellationToken ct)
        => Ok(await _service.GetByStatusAsync(ct));

    /// <summary>Budget vs actual for a period. Both dates are required.</summary>
    [HttpGet("budget-status")]
    public async Task<ActionResult<IReadOnlyList<BudgetStatusDto>>> BudgetStatus(
        [FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd, CancellationToken ct)
        => Ok(await _service.GetBudgetStatusAsync(periodStart, periodEnd, ct));

    /// <summary>Only departments/categories that have exceeded their budget.</summary>
    [HttpGet("over-budget")]
    public async Task<ActionResult<IReadOnlyList<BudgetStatusDto>>> OverBudget(
        [FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd, CancellationToken ct)
        => Ok(await _service.GetOverBudgetAsync(periodStart, periodEnd, ct));

    /// <summary>Per-manager count and value of expenses waiting for approval.</summary>
    [HttpGet("pending-approval-summary")]
    public async Task<ActionResult<IReadOnlyList<PendingApprovalSummaryDto>>> PendingApprovalSummary(
        CancellationToken ct)
        => Ok(await _service.GetPendingApprovalSummaryAsync(ct));

    /// <summary>Average/min/max days from Submitted to Approved. Optional date window on SubmittedDate.</summary>
    [HttpGet("reimbursement-lag")]
    public async Task<ActionResult<ReimbursementLagDto>> ReimbursementLag(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await _service.GetReimbursementLagAsync(from, to, ct));
}
