using ExpenseTracker.Application.Auth;
using ExpenseTracker.Application.Auth.Dtos;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly JwtTokenGenerator _tokenGenerator;

    public AuthService(UserManager<User> userManager, JwtTokenGenerator tokenGenerator)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result<AuthResponse>.Validation("Invalid email or password.");

        // CheckPasswordAsync hashes the input and compares — never compares plaintext.
        var passwordOk = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordOk)
            return Result<AuthResponse>.Validation("Invalid email or password.");

        // A user could theoretically have multiple roles; we take the first.
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Employee";

        var (token, expiresAt) = _tokenGenerator.Generate(user, role);

        return Result<AuthResponse>.Success(new AuthResponse(
            token, expiresAt, user.Id, user.Name, user.Email ?? "", role));
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return Result<AuthResponse>.Conflict($"A user with email {request.Email} already exists.");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            UserName = request.Email,
            Role = request.Role,
            Department = request.Department,
            ManagerId = request.ManagerId,
            EmailConfirmed = true
        };

        var created = await _userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
        {
            var errors = string.Join("; ", created.Errors.Select(e => e.Description));
            return Result<AuthResponse>.Validation(errors);
        }

        await _userManager.AddToRoleAsync(user, request.Role.ToString());

        var (token, expiresAt) = _tokenGenerator.Generate(user, request.Role.ToString());

        return Result<AuthResponse>.Success(new AuthResponse(
            token, expiresAt, user.Id, user.Name, user.Email ?? "", request.Role.ToString()));
    }
}