namespace ExpenseTracker.Application.Auth.Dtos;

// What a successful login returns: the token plus basic user info
// so the client doesn't need a second request to know who it is.
public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    Guid UserId,
    string Name,
    string Email,
    string Role);