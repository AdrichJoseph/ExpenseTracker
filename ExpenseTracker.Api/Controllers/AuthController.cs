using ExpenseTracker.Application.Auth;
using ExpenseTracker.Application.Auth.Dtos;
using ExpenseTracker.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(request, ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : Unauthorized(result.Error);
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _auth.RegisterAsync(request, ct);
        if (result.IsSuccess) return Ok(result.Value);

        return result.ErrorKind switch
        {
            ResultError.Conflict => Conflict(result.Error),
            _                    => BadRequest(result.Error),
        };
    }
}