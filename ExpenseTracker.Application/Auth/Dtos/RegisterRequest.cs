using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Auth.Dtos;

public record RegisterRequest(
    string Name,
    string Email,
    string Password,
    Role Role,
    string? Department,
    Guid? ManagerId);