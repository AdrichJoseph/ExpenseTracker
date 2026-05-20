using System.Security.Claims;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Expenses;
using ExpenseTracker.Application.Expenses.Dtos;
using ExpenseTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _service;

    public ExpensesController(IExpenseService service) => _service = service;

    // Reads the caller's id and role out of the JWT claims.
    private CurrentUser GetCaller()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Token missing user id claim.");
        var roleClaim = User.FindFirstValue(ClaimTypes.Role) ?? "Employee";

        return new CurrentUser(Guid.Parse(idClaim), Enum.Parse<Role>(roleClaim));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExpenseDto>>> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(GetCaller(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseDto>> GetById(Guid id, CancellationToken ct)
        => MapResult(await _service.GetByIdAsync(id, GetCaller(), ct));

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create(
        [FromBody] CreateExpenseRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, GetCaller(), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : MapResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExpenseDto>> Update(
        Guid id, [FromBody] UpdateExpenseRequest request, CancellationToken ct)
        => MapResult(await _service.UpdateAsync(id, request, GetCaller(), ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, GetCaller(), ct);
        if (result.IsSuccess) return NoContent();
        return result.ErrorKind == ResultError.NotFound
            ? NotFound(result.Error)
            : BadRequest(result.Error);
    }

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