using System.Security.Claims;
using ExpenseTracker.Application.Approvals;
using ExpenseTracker.Application.Approvals.Dtos;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Expenses.Dtos;
using ExpenseTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/approvals")]
public class ApprovalsController : ControllerBase
{
    private readonly IApprovalService _service;

    public ApprovalsController(IApprovalService service) => _service = service;

    private CurrentUser GetCaller()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Token missing user id claim.");
        var roleClaim = User.FindFirstValue(ClaimTypes.Role) ?? "Employee";
        return new CurrentUser(Guid.Parse(idClaim), Enum.Parse<Role>(roleClaim));
    }

    /// <summary>
    /// Returns all Submitted expenses the current user can act on (Manager/Admin).
    /// Employees will receive an empty list.
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<IReadOnlyList<ExpenseDto>>> GetPending(CancellationToken ct)
        => Ok(await _service.GetPendingAsync(GetCaller(), ct));

    /// <summary>POST /api/approvals/{id}/approve</summary>
    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ExpenseDto>> Approve(
        Guid id, [FromBody] ApprovalActionRequest request, CancellationToken ct)
        => MapResult(await _service.ApproveAsync(id, request.Comment, GetCaller(), ct));

    /// <summary>POST /api/approvals/{id}/reject  — Comment is the rejection reason and is required.</summary>
    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<ExpenseDto>> Reject(
        Guid id, [FromBody] ApprovalActionRequest request, CancellationToken ct)
        => MapResult(await _service.RejectAsync(id, request.Comment ?? string.Empty, GetCaller(), ct));

    private ActionResult<T> MapResult<T>(Result<T> result)
    {
        if (result.IsSuccess) return Ok(result.Value);
        return result.ErrorKind switch
        {
            ResultError.NotFound   => NotFound(result.Error),
            ResultError.Conflict   => Conflict(result.Error),
            ResultError.Validation => BadRequest(result.Error),
            _                      => BadRequest(result.Error),
        };
    }
}
