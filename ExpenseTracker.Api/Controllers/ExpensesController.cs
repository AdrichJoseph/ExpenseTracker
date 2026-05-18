using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.Expenses;
using ExpenseTracker.Application.Expenses.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _service;

    public ExpensesController(IExpenseService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExpenseDto>>> GetAll(CancellationToken ct)
    {
        var expenses = await _service.GetAllAsync(ct);
        return Ok(expenses);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return MapResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create(
        [FromBody] CreateExpenseRequest request,
        CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);

        if (!result.IsSuccess)
            return MapResult(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExpenseDto>> Update(
        Guid id,
        [FromBody] UpdateExpenseRequest request,
        CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, request, ct);
        return MapResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorKind switch
            {
                ResultError.NotFound => NotFound(result.Error),
                _                    => BadRequest(result.Error),
            };
        }

        return NoContent();
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
