using ExpenseTracker.Application.Auth.Dtos;
using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Application.Auth;

public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
}