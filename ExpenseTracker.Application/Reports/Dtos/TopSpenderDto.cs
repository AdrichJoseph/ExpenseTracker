namespace ExpenseTracker.Application.Reports.Dtos;

public record TopSpenderDto(
    Guid UserId,
    string UserName,
    string Department,
    decimal Total,
    int Count);
